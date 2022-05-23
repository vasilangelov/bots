namespace BOTS.Services.Data.Bets
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using System.Data;

    using BOTS.Data;
    using BOTS.Services.Data.TradingWindows;
    using BOTS.Services.Data.Users;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Data.Repositories;

    public class BetService : IBetService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IRepository<Bet> betRepository;
        private readonly IUserService userService;
        private readonly ITradingWindowService tradingWindowService;
        private readonly ICurrencyPairService currencyPairService;
        private readonly IMapper mapper;

        public BetService(
            ApplicationDbContext dbContext,
            IRepository<Bet> betRepository,
            IUserService userService,
            ITradingWindowService tradingWindowService,
            ICurrencyPairService currencyPairService,
            IMapper mapper)
        {
            this.dbContext = dbContext;
            this.betRepository = betRepository;
            this.userService = userService;
            this.tradingWindowService = tradingWindowService;
            this.currencyPairService = currencyPairService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<T>> GetActiveBetsAsync<T>(string userId)
            => await this.betRepository
                         .AllAsNotracking()
                         .Where(x => x.UserId == userId &&
                                     x.TradingWindow.End > DateTime.UtcNow)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .ToArrayAsync();

        public async Task<T> PlaceBetAsync<T>(
            string userId,
            BetType betType,
            string tradingWindowId,
            byte barrierIndex,
            decimal payout)
        {
            var transaction = await this.dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

            // TODO: check barrier validity...
            // TODO: check money avalability... (money treasury)
            try
            {
                var entryPercentage =
                    await this.tradingWindowService.GetEntryPercentageAsync(
                        tradingWindowId,
                        betType,
                        barrierIndex);

                // TODO IDEA: may have minimal pay... (e.g. 5% of payout...)
                if (0 >= entryPercentage || entryPercentage >= 1)
                {
                    throw new InvalidOperationException("Entry percentage must be a number between 0 and 1");
                }

                var entryFee = payout * entryPercentage;

                bool hasEnoughBalance = await this.userService.SubtractFromBalanceAsync(userId, entryFee);

                if (!hasEnoughBalance)
                {
                    throw new InvalidOperationException("User does not have enough balance");
                }

                int? id = await this.tradingWindowService.GetCurrencyPairIdAsync(tradingWindowId);

                if (!id.HasValue)
                {
                    throw new ArgumentException("Trading window does not exist", nameof(tradingWindowId));
                }

                bool hasActiveCurrencyPairBet = await this.userService.HasActiveBetForCurrencyPairAsync(userId, id.Value);

                if (hasActiveCurrencyPairBet)
                {
                    throw new InvalidOperationException("User has active bet on this currency pair");
                }

                bool isTradingWindowActive = await this.tradingWindowService.IsTradingWindowActiveAsync(tradingWindowId);

                if (!isTradingWindowActive)
                {
                    throw new InvalidOperationException("Trading window is closed");
                }

                decimal? barrier = await this.tradingWindowService.GetBarrierAsync(tradingWindowId, barrierIndex);

                var result =
                    await this.AddBetAsync<T>(
                        userId,
                        tradingWindowId,
                        betType,
                        barrier.Value,
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

        public async Task RewardWinningBetsAsync(
            string tradingWindowId,
            decimal currencyRate)
        {
            
        }

        private async Task<T> AddBetAsync<T>(
            string userId,
            string tradingWindowId,
            BetType betType,
            decimal barrier,
            decimal entryFee,
            decimal payout)
        {
            var bet = new Bet
            {
                UserId = userId,
                TradingWindowId = tradingWindowId,
                Type = betType,
                EntryFee = entryFee,
                BarrierIndex = barrier,
                Payout = payout,
            };

            await this.betRepository.AddAsync(bet);
            await this.betRepository.SaveChangesAsync();

            return await this.betRepository
                .AllAsNotracking()
                .Where(x => x.Id == bet.Id)
                .ProjectTo<T>(this.mapper.ConfigurationProvider)
                .FirstAsync();
        }
    }
}
