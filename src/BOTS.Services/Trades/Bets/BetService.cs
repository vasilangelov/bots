namespace BOTS.Services.Trades.Bets
{
    using System.Data;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Data.Infrastructure.Repositories;
    using BOTS.Data.Infrastructure.Transactions;
    using BOTS.Data.Models;
    using BOTS.Services.ApplicationSettings;
    using BOTS.Services.Balance;
    using BOTS.Services.Balance.System;
    using BOTS.Services.Common.Results;
    using BOTS.Services.Currencies.CurrencyRates;

    using static BOTS.Services.Trades.Bets.BarrierActions;

    [TransientService]
    public class BetService : IBetService
    {
        private readonly IRepository<Bet> betRepository;
        private readonly IRepository<BettingOption> bettingOptionRepository;
        private readonly ICurrencyRateProviderService currencyRateProviderService;
        private readonly IBalanceService balanceService;
        private readonly ITreasuryService treasuryService;
        private readonly IApplicationSettingService applicationSettingService;
        private readonly ITransactionManager transactionManager;
        private readonly IMapper mapper;

        public BetService(
            IRepository<Bet> betRepository,
            IRepository<BettingOption> bettingOptionRepository,
            ICurrencyRateProviderService currencyRateProviderService,
            IBalanceService balanceService,
            ITreasuryService treasuryService,
            IApplicationSettingService applicationSettingService,
            ITransactionManager transactionManager,
            IMapper mapper)
        {
            this.betRepository = betRepository;
            this.bettingOptionRepository = bettingOptionRepository;
            this.currencyRateProviderService = currencyRateProviderService;
            this.balanceService = balanceService;
            this.treasuryService = treasuryService;
            this.applicationSettingService = applicationSettingService;
            this.transactionManager = transactionManager;
            this.mapper = mapper;
        }

        public async Task<T> GetBetAsync<T>(Guid betId)
            => await this.betRepository
                         .AllAsNoTracking()
                         .Where(x => x.Id == betId)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .FirstAsync();

        public async Task<IEnumerable<T>> GetActiveUserBetsAsync<T>(Guid userId)
            => await this.betRepository
                         .AllAsNoTracking()
                         .Where(x => x.UserId == userId &&
                                     !x.BettingOption.TradingWindow.IsClosed)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .ToArrayAsync();

        public async Task<IEnumerable<T>> GetUserBetHistoryAsync<T>(Guid userId, int skip, int take)
            => await this.betRepository
                            .AllAsNoTracking()
                            .Where(x => x.UserId == userId && x.BettingOption.TradingWindow.IsClosed)
                            .OrderByDescending(x => x.BettingOption.TradingWindow.End)
                            .Skip(skip)
                            .Take(take)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .ToArrayAsync();

        public async Task<int> GetUserHistoryPageCount(Guid userId, int itemsPerPage)
        {
            var endedBetsCount = await this.betRepository
                                       .AllAsNoTracking()
                                       .Where(x => x.UserId == userId && x.BettingOption.TradingWindow.IsClosed)
                                       .CountAsync();

            return (int)Math.Ceiling(endedBetsCount / (double)itemsPerPage);
        }

        public async Task<Result<Guid>> PlaceBetAsync(
            Guid? userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout)
        {
            var transaction = await this.transactionManager.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                if (!userId.HasValue)
                {
                    return Result.Error("InvalidUser");
                }

                var maximumPayout = await applicationSettingService.GetValueAsync<decimal>("MaximumPayout");
                var minimumPayout = await applicationSettingService.GetValueAsync<decimal>("MinimumPayout");

                if (minimumPayout > payout || payout > maximumPayout)
                {
                    return Result.Error("PayoutRange", minimumPayout, maximumPayout);
                }

                var bettingOption = await this.GetBettingOptionAsync(bettingOptionId);

                if (!bettingOption.Barriers.Contains(barrier))
                {
                    return Result.Error("Invalid barrier value");
                }

                var currencyRate = await this.currencyRateProviderService
                    .GetCurrencyRateAsync(bettingOption.CurrencyPairId);

                var entryPercentage = GetEntryPercentage(bettingOption.Barriers,
                                                         barrier,
                                                         bettingOption.BarrierStep,
                                                         currencyRate,
                                                         bettingOption.TimeRemaining,
                                                         bettingOption.FullTime,
                                                         betType);

                if (0 >= entryPercentage || entryPercentage >= 1)
                {
                    return Result.Error("EntryPercentageRange", 0, 1);
                }

                var entryFee = payout * entryPercentage;

                bool hasActiveCurrencyPairBet =
                    await this.HasActiveUserCurrencyPairBet(userId.Value, bettingOption.CurrencyPairId);

                if (hasActiveCurrencyPairBet)
                {
                    return Result.Error("ActiveUserBet");
                }

                bool isTradingWindowActive = bettingOption.TimeRemaining > 0;

                if (!isTradingWindowActive)
                {
                    return Result.Error("ClosedTradingWindow");
                }

                bool hasEnoughBalance = await this.balanceService.HasEnoughBalanceAsync(userId.Value, entryFee);

                if (!hasEnoughBalance)
                {
                    return Result.Error("InsufficientBalance");
                }

                bool canPayout = await this.treasuryService.CanPlaceBetAsync(entryFee, payout);

                if (!canPayout)
                {
                    return Result.Error("CannotPayoutReward");
                }

                await this.treasuryService.AddSystemBalanceAsync(entryFee);
                await this.treasuryService.AddUserProfitsAsync(payout - entryFee);
                await this.balanceService.SubtractFromBalanceAsync(userId.Value,
                                                                   entryFee);

                var betId = await this.AddBetAsync(userId.Value,
                                                   bettingOptionId,
                                                   betType,
                                                   barrier,
                                                   entryFee,
                                                   payout);

                await transaction.CommitAsync();

                return betId;
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task PayoutBetsAsync(Guid tradingWindowId)
        {
            var winningBets = await this.betRepository
                .AllAsNoTracking()
                .Where(x => x.BettingOption.TradingWindow.IsClosed &&
                            x.BettingOption.TradingWindowId == tradingWindowId &&
                            ((x.Type == BetType.Higher &&
                                x.BettingOption.CloseValue > x.BarrierPrediction) ||
                            (x.Type == BetType.Lower &&
                                x.BettingOption.CloseValue < x.BarrierPrediction)))
                .Select(x => new { x.Payout, x.UserId })
                .ToArrayAsync();

            foreach (var winningBet in winningBets)
            {
                await this.balanceService.AddToBalanceAsync(winningBet.UserId, winningBet.Payout);
            }

            var losingBets = await this.betRepository
                .AllAsNoTracking()
                .Where(x => x.BettingOption.TradingWindow.IsClosed &&
                            x.BettingOption.TradingWindowId == tradingWindowId &&
                            ((x.Type == BetType.Higher &&
                                x.BarrierPrediction >= x.BettingOption.CloseValue) ||
                            (x.Type == BetType.Lower &&
                                x.BarrierPrediction <= x.BettingOption.CloseValue)))
                .SumAsync(x => x.Payout - x.EntryFee);

            if (losingBets > 0)
            {
                await this.treasuryService.SubtractUserProfitsAsync(losingBets);
            }
        }

        private async Task<Guid> AddBetAsync(
            Guid userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal entryFee,
            decimal payout)
        {
            var bet = new Bet
            {
                UserId = userId,
                BettingOptionId = bettingOptionId,
                Type = betType,
                EntryFee = entryFee,
                BarrierPrediction = barrier,
                Payout = payout,
            };

            await this.betRepository.AddAsync(bet);
            await this.betRepository.SaveChangesAsync();

            return bet.Id;
        }

        private async Task<bool> HasActiveUserCurrencyPairBet(Guid userId, int currencyPairId)
            => await this.betRepository
                            .AllAsNoTracking()
                            .AnyAsync(x => x.UserId == userId &&
                                           x.BettingOption.CurrencyPairId == currencyPairId &&
                                           !x.BettingOption.TradingWindow.IsClosed);

        private record class BettingOptionInfo(int CurrencyPairId,
                                               decimal[] Barriers,
                                               decimal BarrierStep,
                                               long FullTime,
                                               long TimeRemaining);

        private async Task<BettingOptionInfo> GetBettingOptionAsync(Guid bettingOptionId)
            => await this.bettingOptionRepository
                            .AllAsNoTracking()
                            .Where(x => x.Id == bettingOptionId)
                            .Select(x => new BettingOptionInfo(
                                x.CurrencyPairId,
                                x.Barriers,
                                x.BarrierStep,
                                (long)x.TradingWindow.Duration.TotalSeconds,
                                (long)x.TradingWindow.End.Subtract(DateTime.UtcNow).TotalSeconds))
                            .FirstAsync();
    }
}
