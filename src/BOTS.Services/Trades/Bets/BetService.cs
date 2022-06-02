namespace BOTS.Services.Trades.Bets
{
    using System.Data;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Data.Infrastructure.Repositories;
    using BOTS.Data.Infrastructure.Transactions;
    using BOTS.Data.Models;
    using BOTS.Services.Balance;
    using BOTS.Services.Balance.System;
    using BOTS.Services.Common;
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
        private readonly ITransactionManager transactionManager;
        private readonly IMapper mapper;

        public BetService(
            IRepository<Bet> betRepository,
            IRepository<BettingOption> bettingOptionRepository,
            ICurrencyRateProviderService currencyRateProviderService,
            IBalanceService balanceService,
            ITreasuryService treasuryService,
            ITransactionManager transactionManager,
            IMapper mapper)
        {
            this.betRepository = betRepository;
            this.bettingOptionRepository = bettingOptionRepository;
            this.currencyRateProviderService = currencyRateProviderService;
            this.balanceService = balanceService;
            this.treasuryService = treasuryService;
            this.transactionManager = transactionManager;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<T>> GetActiveUserBetsAsync<T>(Guid userId)
            => await this.betRepository
                         .AllAsNoTracking()
                         .Where(x => x.UserId == userId &&
                                     !x.BettingOption.TradingWindow.IsClosed)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .ToArrayAsync();

        public async Task<T> PlaceBetAsync<T>(
            Guid userId,
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout)
        {
            var transaction = await this.transactionManager.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                var bettingOption = await this.GetBettingOptionAsync(bettingOptionId);

                if (!bettingOption.Barriers.Contains(barrier))
                {
                    throw new ArgumentException("Invalid barrier value", nameof(barrier));
                }

                var currencyRate =
                    await this.currencyRateProviderService.GetCurrencyRateAsync(bettingOption.CurrencyPairId);

                var entryPercentage = GetEntryPercentage(bettingOption.Barriers,
                                                         barrier,
                                                         bettingOption.BarrierStep,
                                                         currencyRate,
                                                         bettingOption.TimeRemaining,
                                                         bettingOption.FullTime,
                                                         betType);

                if (0 >= entryPercentage || entryPercentage >= 1)
                {
                    throw new InvalidOperationException("Entry percentage must be a number between 0 and 1");
                }

                var entryFee = payout * entryPercentage;

                bool hasActiveCurrencyPairBet =
                    await this.HasActiveUserCurrencyPairBet(userId, bettingOption.CurrencyPairId);

                if (hasActiveCurrencyPairBet)
                {
                    throw new InvalidOperationException("User has active bet on this currency pair");
                }

                bool isTradingWindowActive = bettingOption.TimeRemaining > 0;

                if (!isTradingWindowActive)
                {
                    throw new InvalidOperationException("Trading window is closed");
                }

                bool hasEnoughBalance = await this.balanceService.HasEnoughBalanceAsync(userId, entryFee);

                if (!hasEnoughBalance)
                {
                    throw new InvalidOperationException("User does not have enough balance");
                }

                bool canPayout = await this.treasuryService.CanPlaceBetAsync(entryFee, payout);

                if (!canPayout)
                {
                    throw new InvalidOperationException("Cannot payout potential reward");
                }

                await this.treasuryService.AddSystemBalanceAsync(entryFee);
                await this.treasuryService.AddUserProfitsAsync(payout - entryFee);
                await this.balanceService.SubtractFromBalanceAsync(userId, entryFee);

                var result = await this.AddBetAsync<T>(userId,
                                                       bettingOptionId,
                                                       betType,
                                                       barrier,
                                                       entryFee,
                                                       payout);

                await transaction.CommitAsync();

                return result;
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
                .Where(x => x.BettingOption.TradingWindowId == tradingWindowId &&
                            ((x.Type == BetType.Higher &&
                                x.BarrierPrediction < x.BettingOption.CloseValue) ||
                            (x.Type == BetType.Lower &&
                                x.BarrierPrediction > x.BettingOption.CloseValue)))
                .Select(x => new { x.Payout, x.UserId })
                .ToArrayAsync();

            foreach (var winningBet in winningBets)
            {
                await this.balanceService.AddToBalanceAsync(winningBet.UserId, winningBet.Payout);
            }

            var losingBets = await this.betRepository
                .AllAsNoTracking()
                .Where(x => x.BettingOption.TradingWindowId == tradingWindowId &&
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

        public async Task<IEnumerable<T>> GetUserBetHistoryAsync<T>(Guid userId, int skip, int take)
            => await this.betRepository
                            .AllAsNoTracking()
                            .Where(x => x.UserId == userId && x.BettingOption.TradingWindow.IsClosed)
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

        private async Task<T> AddBetAsync<T>(
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

            return await this.betRepository
                    .AllAsNoTracking()
                    .Where(x => x.Id == bet.Id)
                    .ProjectTo<T>(this.mapper.ConfigurationProvider)
                    .FirstAsync();
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
