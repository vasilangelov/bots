namespace BOTS.Services.Data.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.Extensions.Caching.Memory;

    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Models;

    public class TradingWindowService : ITradingWindowService
    {
        private readonly IRepository<TradingWindow> tradingWindowRepository;
        private readonly ICurrencyPairService currencyPairService;
        private readonly ITradingWindowOptionService tradingWindowOptionService;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public TradingWindowService(IRepository<TradingWindow> tradingWindowRepository,
                                    ICurrencyPairService currencyPairService,
                                    ITradingWindowOptionService tradingWindowOptionService,
                                    IMapper mapper,
                                    IMemoryCache memoryCache)
        {
            this.tradingWindowRepository = tradingWindowRepository;
            this.currencyPairService = currencyPairService;
            this.tradingWindowOptionService = tradingWindowOptionService;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(
            int currencyPairId,
            CancellationToken cancellationToken = default)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => x.CurrencyPairId == currencyPairId &&
                                    x.End > DateTime.UtcNow)
                        .OrderBy(x => x.Option.Duration)
                        .ProjectTo<T>(this.mapper.ConfigurationProvider)
                        .ToArrayAsync(cancellationToken);

        public async Task<T> GetTradingWindowAsync<T>(string tradingWindowId, CancellationToken cancellationToken = default)
            => await this.tradingWindowRepository
                         .AllAsNotracking()
                         .Where(x => x.Id == tradingWindowId)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .FirstAsync(cancellationToken);

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

        public async Task<bool> IsTradingWindowActiveAsync(string tradingWindowId, CancellationToken cancellationToken = default)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .AnyAsync(x => x.Id == tradingWindowId && x.End > DateTime.UtcNow, cancellationToken);

        public async Task EnsureAllTradingWindowsActiveAsync(
            IEnumerable<int> currencyPairIds,
            CancellationToken cancellationToken = default)
        {
            bool areTradingWindowsLoaded = this.memoryCache.TryGetValue("TradingWindows", out ICollection<TradingWindow> tradingWindows);

            if (!areTradingWindowsLoaded)
            {
                tradingWindows = await this.tradingWindowRepository
                                    .AllAsNotracking()
                                    .Where(x => x.End > DateTime.UtcNow)
                                    .ToListAsync(cancellationToken);
            }

            var tradingWindowOptions = await this.tradingWindowOptionService.GetAllTradingWindowOptionsAsync<TradingWindowOptionInfo>(cancellationToken);

            var endedTradingWindows = tradingWindows.Where(x => x.End <= DateTime.UtcNow).ToArray();

            foreach (var tradingWindow in endedTradingWindows)
            {
                decimal currencyRate = await this.currencyPairService.GetCurrencyRateAsync(tradingWindow.CurrencyPairId, cancellationToken);

                tradingWindow.ClosingPrice = currencyRate;

                tradingWindows.Remove(tradingWindow);

                await this.UpdateTradingWindowAsync(tradingWindow);
            }

            foreach (var currencyPairId in currencyPairIds)
            {
                foreach (var tradingWindowOption in tradingWindowOptions)
                {
                    bool isTradingWindowActive = tradingWindows.Any(x => x.CurrencyPairId == currencyPairId && x.OptionId == tradingWindowOption.Id);

                    if (!isTradingWindowActive)
                    {
                        var tradingWindow = await this.CreateTradingWindowAsync(
                                                            currencyPairId,
                                                            tradingWindowOption.Id,
                                                            tradingWindowOption.Duration,
                                                            cancellationToken);

                        tradingWindows.Add(tradingWindow);
                    }
                }
            }

            this.memoryCache.Set("TradingWindows", tradingWindows);
        }

        private async Task<TradingWindow> CreateTradingWindowAsync(int currencyPairId,
                                                    int tradingWindowOptionId,
                                                    TimeSpan duration,
                                                    CancellationToken cancellationToken = default)
        {
            var openingPrice = await this.currencyPairService
                                        .GetCurrencyRateAsync(currencyPairId, cancellationToken);

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

        private async Task UpdateTradingWindowAsync(TradingWindow tradingWindow)
        {
            this.tradingWindowRepository.Update(tradingWindow);
            await this.tradingWindowRepository.SaveChangesAsync();
        }
    }
}
