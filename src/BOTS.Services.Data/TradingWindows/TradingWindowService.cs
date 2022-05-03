namespace BOTS.Services.Data.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
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
