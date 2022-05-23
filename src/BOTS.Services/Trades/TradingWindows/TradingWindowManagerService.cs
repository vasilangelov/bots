namespace BOTS.Services.Trades.TradingWindows
{
    using BOTS.Data.Models;
    using BOTS.Services.Common;
    using BOTS.Services.Infrastructure.Extensions;
    using BOTS.Services.Trades.Bets;

    using Microsoft.Extensions.Caching.Memory;

    [TransientService]
    public class TradingWindowManagerService : ITradingWindowManagerService
    {
        private static readonly object tradingWindowsKey = new();
        private static readonly object bettingOptionPresetsKey = new();

        private readonly IRepository<TradingWindow> tradingWindowRepository;
        private readonly IRepository<BettingOptionPreset> bettingOptionPresetRepository;
        private readonly IBettingOptionService bettingOptionService;
        private readonly IBetService betService;
        private readonly IMemoryCache memoryCache;

        public TradingWindowManagerService(
            IRepository<TradingWindow> tradingWindowRepository,
            IRepository<BettingOptionPreset> bettingOptionPresetRepository,
            IBettingOptionService bettingOptionService,
            IBetService betService,
            IMemoryCache memoryCache)
        {
            this.tradingWindowRepository = tradingWindowRepository;
            this.bettingOptionPresetRepository = bettingOptionPresetRepository;
            this.bettingOptionService = bettingOptionService;
            this.betService = betService;
            this.memoryCache = memoryCache;
        }

        public async Task EnsureAllTradingWindowsActiveAsync()
        {
            var tradingWindows = this.memoryCache.GetOrAdd(tradingWindowsKey,
                                                           this.GetActiveTradingWindows);

            var now = DateTime.UtcNow;

            var endedWindows = tradingWindows.Where(x => x.End <= now).ToArray();

            foreach (var endedWindow in endedWindows)
            {
                await this.CloseTradingWindowAsync(endedWindow.Id, endedWindow.End);

                tradingWindows.Remove(endedWindow);
            }

            var bettingOptionPresets = this.GetBettingOptionPresetsAsync();

            foreach (var bettingOption in bettingOptionPresets)
            {
                var activeBettingOptions = tradingWindows
                    .Where(x => x.Duration == bettingOption.Key)
                    .SelectMany(x => x.BettingOptions)
                    .ToArray();

                var presetsToCreate = bettingOption
                        .Where(x => !activeBettingOptions
                            .Any(y => x.CurrencyPairId == y.CurrencyPairId))
                        .ToArray();

                if (presetsToCreate.Length > 0)
                {
                    var tradingWindow = await this.CreateTradingWindowAsync(bettingOption.Key);

                    await this.bettingOptionService
                                .BulkCreateBettingOptionsAsync(tradingWindow.Id,
                                                               presetsToCreate);

                    tradingWindows.Add(new TradingWindowInfo(
                        tradingWindow.Id,
                        bettingOption.Key,
                        tradingWindow.End,
                        presetsToCreate.Select(x => new BettingOptionInfo(x.CurrencyPairId))));
                }
            }
        }

        public async Task UpdateEndedTradingWindowsAsync()
        {
            var unsetEndedTradingWindows = await this.GetNotClosedEndedTradingWindowsAsync();

            foreach (var tradingWindow in unsetEndedTradingWindows)
            {
                await this.bettingOptionService.SetBettingOptionsClosingValueAsync(
                    tradingWindow.Id,
                    tradingWindow.End);
            }
        }

        private record class TradingWindowInfo(Guid Id,
                                               TimeSpan Duration,
                                               DateTime End,
                                               IEnumerable<BettingOptionInfo> BettingOptions);

        private record class EndedTradingWindowInfo(Guid Id, DateTime End);

        private record class BettingOptionInfo(int CurrencyPairId);

        private IEnumerable<IGrouping<TimeSpan, BettingOptionPreset>> GetBettingOptionPresetsAsync()
            => this.memoryCache
                .GetOrCreate(
                    bettingOptionPresetsKey,
                    (entry) =>
                    {
                        entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

                        return this.bettingOptionPresetRepository
                                        .AllAsNotracking()
                                        .Include(x => x.TradingWindowPreset)
                                        .AsEnumerable()
                                        .GroupBy(x => x.TradingWindowPreset.Duration)
                                        .ToArray();
                    });

        private ICollection<TradingWindowInfo> GetActiveTradingWindows()
            => this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => !x.IsClosed)
                        .Select(x => new TradingWindowInfo(
                            x.Id,
                            x.Duration,
                            x.End,
                            x.BettingOptions
                                .Select(x => new BettingOptionInfo(x.CurrencyPairId))))
                        .ToHashSet();

        private async Task<IEnumerable<EndedTradingWindowInfo>> GetNotClosedEndedTradingWindowsAsync()
            => await this.tradingWindowRepository
                            .AllAsNotracking()
                            .Where(x => DateTime.UtcNow >= x.End && !x.IsClosed)
                            .Select(x => new EndedTradingWindowInfo(x.Id, x.End))
                            .ToArrayAsync();

        private async Task<TradingWindow> CreateTradingWindowAsync(TimeSpan duration)
        {
            var start = DateTime.UtcNow;

            var model = new TradingWindow
            {
                IsClosed = false,
                Start = start,
                Duration = duration,
                End = start.Add(duration),
            };

            await this.tradingWindowRepository.AddAsync(model);
            await this.tradingWindowRepository.SaveChangesAsync();

            return model;
        }

        private async Task CloseTradingWindowAsync(Guid tradingWindowId, DateTime end)
        {
            var window = new TradingWindow
            {
                Id = tradingWindowId,
                IsClosed = true,
            };

            this.tradingWindowRepository.Patch(window, x => x.IsClosed);

            await this.tradingWindowRepository.SaveChangesAsync();

            await this.bettingOptionService.SetBettingOptionsClosingValueAsync(tradingWindowId, end);

            await this.betService.PayoutBetsAsync(tradingWindowId);
        }
    }
}
