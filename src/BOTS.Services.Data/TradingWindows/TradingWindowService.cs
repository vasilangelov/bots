namespace BOTS.Services.Data.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.Extensions.Caching.Memory;

    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Models;
    using BOTS.Services.Infrastructure.Extensions;
    using BOTS.Services.Data.Users;

    public class TradingWindowService : ITradingWindowService
    {
        private static readonly object tradingWindowsKey = new();

        private readonly IRepository<TradingWindow> tradingWindowRepository;
        private readonly IUserService userService;
        private readonly ICurrencyPairService currencyPairService;
        private readonly ITradingWindowOptionService tradingWindowOptionService;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public TradingWindowService(IRepository<TradingWindow> tradingWindowRepository,
                                    IUserService userService,
                                    ICurrencyPairService currencyPairService,
                                    ITradingWindowOptionService tradingWindowOptionService,
                                    IMapper mapper,
                                    IMemoryCache memoryCache)
        {
            this.tradingWindowRepository = tradingWindowRepository;
            this.userService = userService;
            this.currencyPairService = currencyPairService;
            this.tradingWindowOptionService = tradingWindowOptionService;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(
            int currencyPairId)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => x.CurrencyPairId == currencyPairId &&
                                    x.End > DateTime.UtcNow)
                        .OrderBy(x => x.Option.Duration)
                        .ProjectTo<T>(this.mapper.ConfigurationProvider)
                        .ToArrayAsync();

        public async Task<T> GetTradingWindowAsync<T>(string tradingWindowId)
            => await this.tradingWindowRepository
                         .AllAsNotracking()
                         .Where(x => x.Id == tradingWindowId)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .FirstAsync();

        public async Task<int?> GetCurrencyPairIdAsync(string tradingWindowId)
            => await this.tradingWindowRepository
                         .AllAsNotracking()
                         .Where(x => x.Id == tradingWindowId)
                         .Select(x => x.CurrencyPairId)
                         .FirstOrDefaultAsync();

        public async Task<decimal> GetEntryPercentageAsync(
            string tradingWindowId,
            BetType betType,
            byte barrierIndex)
        {
            var window = await this.tradingWindowRepository
                 .AllAsNotracking()
                 .Where(x => x.Id == tradingWindowId)
                 .Select(x => new
                 {
                     x.CurrencyPairId,
                     x.Option.BarrierCount,
                     x.Option.BarrierStep,
                     x.OpeningPrice,
                     RemainingTime = (int)x.End.Subtract(DateTime.UtcNow).TotalSeconds,
                     FullTime = (int)x.End.Subtract(x.Start).TotalSeconds
                 })
                 .FirstOrDefaultAsync();

            if (window is null)
            {
                throw new InvalidOperationException("Trading window does not exist");
            }

            var currencyRate = await this.currencyPairService.GetCurrencyRateAsync(window.CurrencyPairId);

            var barrier = this.CalculateBarrier(barrierIndex, window.BarrierCount, window.OpeningPrice, window.BarrierStep);

            var barrierDistance = betType switch
            {
                BetType.Higher => currencyRate - barrier,
                BetType.Lower => barrier - currencyRate,
                _ => throw new InvalidOperationException("Invalid bet type")
            };

            decimal delta = window.BarrierCount * window.BarrierStep;

            return barrierDistance / delta + 0.5m * (2 - window.RemainingTime / (decimal)window.FullTime);
        }

        public decimal CalculateBarrier(
            byte barrierIndex,
            int barrierCount,
            decimal openingPrice,
            decimal barrierStep)
            => openingPrice + (barrierIndex - barrierCount / 2) * barrierStep;

        public async Task<bool> IsTradingWindowActiveAsync(string tradingWindowId)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .AnyAsync(x => x.Id == tradingWindowId && x.End > DateTime.UtcNow);

        public async Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds)
        {
            ICollection<TradingWindow> tradingWindows = this.memoryCache.GetOrAdd(
                tradingWindowsKey,
                this.GetActiveTradingWindows);

            var tradingWindowOptions = await this.tradingWindowOptionService
                    .GetAllTradingWindowOptionsAsync<TradingWindowOptionInfo>();

            var endedTradingWindows = tradingWindows
                                        .Where(x => DateTime.UtcNow >= x.End)
                                        .ToArray();

            foreach (var tradingWindow in endedTradingWindows)
            {
                decimal currencyRate = await this.currencyPairService.GetCurrencyRateAsync(tradingWindow.CurrencyPairId);

                tradingWindows.Remove(tradingWindow);

                await this.UpdateTradingWindowClosingPriceAsync(tradingWindow, currencyRate);
            }

            // TODO: bulk save changes???
            await this.tradingWindowRepository.SaveChangesAsync();

            foreach (var currencyPairId in currencyPairIds)
            {
                foreach (var tradingWindowOption in tradingWindowOptions)
                {
                    bool hasActiveTradingWindow = tradingWindows
                                                    .Any(x => x.CurrencyPairId == currencyPairId &&
                                                              x.OptionId == tradingWindowOption.Id);

                    if (!hasActiveTradingWindow)
                    {
                        var tradingWindow = await this.CreateTradingWindowAsync(
                                                            currencyPairId,
                                                            tradingWindowOption.Id,
                                                            tradingWindowOption.Duration);

                        tradingWindows.Add(tradingWindow);
                    }
                }
            }
        }

        public async Task UpdateEndedTradingWindowsAsync()
        {
            var unsetEndedTradingWindows = await this.GetUnsetEndedTradingWindowsAsync();

            foreach (var tradingWindow in unsetEndedTradingWindows)
            {
                decimal currencyRate = await this.currencyPairService.GetPastCurrencyRateAsync(tradingWindow.CurrencyPairId, tradingWindow.End);

                await this.UpdateTradingWindowClosingPriceAsync(tradingWindow, currencyRate);
            }

            await this.tradingWindowRepository.SaveChangesAsync();
        }

        private async Task UpdateTradingWindowClosingPriceAsync(TradingWindow tradingWindow, decimal closingPrice)
        {
            // TODO: IMPORTANT! calculate circulating money...
            tradingWindow.ClosingPrice = closingPrice;

            this.tradingWindowRepository.Update(tradingWindow);

            await this.UpdateWinningBetsAsync(tradingWindow.Id);
        }

        private async Task UpdateWinningBetsAsync(string tradingWindowId)
        {
            var bets = await this.GetWinningBetsAsync(tradingWindowId);

            foreach (var bet in bets)
            {
                await this.userService.AddToBalanceAsync(bet.UserId, bet.Payout);
            }
        }

        private async Task<IEnumerable<Bet>> GetWinningBetsAsync(string tradingWindowId)
            => await this.tradingWindowRepository
                    .AllAsNotracking()
                    .Where(x => x.Id == tradingWindowId)
                    .SelectMany(x => x.Bets
                        .Where(y =>
                            (y.Type == BetType.Higher &&
                             y.TradingWindow.OpeningPrice + (y.BarrierIndex - y.TradingWindow.Option.BarrierCount / 2) * y.TradingWindow.Option.BarrierStep <= y.TradingWindow.ClosingPrice) ||
                            (y.Type == BetType.Lower && y.TradingWindow.OpeningPrice + (y.BarrierIndex - y.TradingWindow.Option.BarrierCount / 2) * y.TradingWindow.Option.BarrierStep > y.TradingWindow.ClosingPrice)))
                    .ToArrayAsync();

        private async Task<IEnumerable<TradingWindow>> GetUnsetEndedTradingWindowsAsync()
            => await this.tradingWindowRepository
                            .AllAsNotracking()
                            .Where(x => DateTime.UtcNow >= x.End &&
                                        !x.ClosingPrice.HasValue)
                            .ToArrayAsync();

        private ICollection<TradingWindow> GetActiveTradingWindows()
            => this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => DateTime.UtcNow < x.End)
                        .ToHashSet();

        private async Task<TradingWindow> CreateTradingWindowAsync(
            int currencyPairId,
            int tradingWindowOptionId,
            TimeSpan duration)
        {
            var openingPrice = await this.currencyPairService.GetCurrencyRateAsync(currencyPairId);

            var start = DateTime.UtcNow;

            var model = new TradingWindow
            {
                CurrencyPairId = currencyPairId,
                OptionId = tradingWindowOptionId,
                OpeningPrice = openingPrice,
                Start = start,
                End = start.Add(duration)
            };

            await this.tradingWindowRepository.AddAsync(model);
            await this.tradingWindowRepository.SaveChangesAsync();

            return model;
        }
    }
}
