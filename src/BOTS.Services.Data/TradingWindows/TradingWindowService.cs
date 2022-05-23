namespace BOTS.Services.Data.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.Extensions.Caching.Memory;

    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Infrastructure.Extensions;
    using BOTS.Services.Data.Users;
    using BOTS.Common;
    using BOTS.Data.Repositories;

    public class TradingWindowService : ITradingWindowService
    {
        private static readonly object tradingWindowsKey = new();

        public static decimal GetHigherPercentage(
            decimal currencyRate,
            decimal barrier,
            decimal delta,
            long remaining,
            long fullTime)
            => (currencyRate - barrier) / delta + 0.5m * (2 - remaining / (decimal)fullTime);

        public static decimal GetLowerPercentage(
            decimal currencyRate,
            decimal barrier,
            decimal delta,
            long remaining,
            long fullTime)
           => (barrier - currencyRate) / delta + 0.5m * (2 - remaining / (decimal)fullTime);

        private readonly IRepository<TradingWindow> tradingWindowRepository;
        private readonly IRepository<TradingWindowPreset> tradingWindowPresetRepository;
        private readonly IUserService userService;
        private readonly ICurrencyPairService currencyPairService;
        private readonly IMapper mapper;
        private readonly IMemoryCache memoryCache;

        public TradingWindowService(
            IRepository<TradingWindow> tradingWindowRepository,
            IRepository<TradingWindowPreset> tradingWindowPresetRepository,
            IUserService userService,
            ICurrencyPairService currencyPairService,
            IMapper mapper,
            IMemoryCache memoryCache)
        {
            this.tradingWindowRepository = tradingWindowRepository;
            this.tradingWindowPresetRepository = tradingWindowPresetRepository;
            this.userService = userService;
            this.currencyPairService = currencyPairService;
            this.mapper = mapper;
            this.memoryCache = memoryCache;
        }

        public async Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(
            int currencyPairId)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => x.CurrencyPairId == currencyPairId &&
                                    x.End > DateTime.UtcNow)
                        .OrderBy(x => x.Setting.Duration)
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
                     x.Setting.BarrierCount,
                     x.Setting.BarrierStep,
                     x.Barriers,
                     RemainingTime = (int)x.End.Subtract(DateTime.UtcNow).TotalSeconds,
                     FullTime = (int)x.End.Subtract(x.Start).TotalSeconds
                 })
                 .FirstOrDefaultAsync();

            if (window is null)
            {
                throw new InvalidOperationException("Trading window does not exist");
            }

            var currencyRate = await this.currencyPairService.GetCurrencyRateAsync(window.CurrencyPairId);

            var barrier = window.Barriers[barrierIndex];

            var barrierDistance = betType switch
            {
                BetType.Higher => currencyRate - barrier,
                BetType.Lower => barrier - currencyRate,
                _ => throw new InvalidOperationException("Invalid bet type")
            };

            decimal delta = window.BarrierCount * window.BarrierStep;

            return barrierDistance / delta + 0.5m * (2 - window.RemainingTime / (decimal)window.FullTime);
        }

        public async Task<bool> IsTradingWindowActiveAsync(string tradingWindowId)
            => await this.tradingWindowRepository
                        .AllAsNotracking()
                        .AnyAsync(x => x.Id == tradingWindowId && x.End > DateTime.UtcNow);

        public async Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds)
        {
            ICollection<TradingWindow> tradingWindows = this.memoryCache.GetOrAdd(
                tradingWindowsKey,
                this.GetActiveTradingWindows);

            var tradingWindowPresets = await this.tradingWindowPresetRepository
                                        .AllAsNotracking()
                                        .Select(x => new
                                        {
                                            x.SettingId,
                                            x.CurrencyPairId,
                                            x.TradingWindowSetting.BarrierCount,
                                            x.TradingWindowSetting.BarrierStep,
                                            x.TradingWindowSetting.Duration,
                                        })
                                        .ToArrayAsync();

            var now = DateTime.UtcNow;

            var endedWindows = tradingWindows.Where(x => x.End <= now).ToArray();

            foreach (var endedWindow in endedWindows)
            {
                decimal currentPrice = await this.currencyPairService.GetCurrencyRateAsync(endedWindow.CurrencyPairId);

                await this.CloseTradingWindowAsync(endedWindow, currentPrice);

                tradingWindows.Remove(endedWindow);
            }

            foreach (var preset in tradingWindowPresets)
            {
                bool tradingWindowExists = tradingWindows.Any(x =>
                                                x.CurrencyPairId == preset.CurrencyPairId &&
                                                x.SettingId == preset.SettingId);

                if (!tradingWindowExists)
                {
                    await this.CreateTradingWindowAsync(preset.CurrencyPairId,
                                                        preset.SettingId,
                                                        preset.BarrierCount,
                                                        preset.BarrierStep,
                                                        preset.Duration);
                }
            }
        }

        public async Task UpdateEndedTradingWindowsAsync()
        {
            var unsetEndedTradingWindows = await this.GetNotClosedEndedTradingWindowsAsync();

            foreach (var tradingWindow in unsetEndedTradingWindows)
            {
                decimal currencyRate = await this.currencyPairService.GetPastCurrencyRateAsync(tradingWindow.CurrencyPairId, tradingWindow.End);

                await this.CloseTradingWindowAsync(tradingWindow, currencyRate);
            }

            await this.tradingWindowRepository.SaveChangesAsync();
        }

        public async Task<decimal?> GetBarrierAsync(string tradingWindowId, byte barrierIndex)
        {
            var barriers = await this.tradingWindowRepository
                                       .AllAsNotracking()
                                       .Where(x => x.Id == tradingWindowId)
                                       .Select(x => x.Barriers)
                                       .FirstOrDefaultAsync();

            if (barriers is not null && barrierIndex < barriers.Length)
            {
                return barriers[barrierIndex];
            }

            return null;
        }

        private async Task CloseTradingWindowAsync(TradingWindow tradingWindow)
        {
            // TODO: IMPORTANT! calculate circulating money...
            tradingWindow.IsClosed = true;

            this.tradingWindowRepository.Update(tradingWindow);

            // TODO: notify 
            // TODO: greda :) circular dependency Bet <-> TradingWindow...
            await this.(tradingWindow.Id);
        }

        private async Task<IEnumerable<TradingWindow>> GetNotClosedEndedTradingWindowsAsync()
            => await this.tradingWindowRepository
                            .AllAsNotracking()
                            .Where(x => DateTime.UtcNow >= x.End && !x.IsClosed)
                            .ToArrayAsync();

        private ICollection<TradingWindow> GetActiveTradingWindows()
            => this.tradingWindowRepository
                        .AllAsNotracking()
                        .Where(x => DateTime.UtcNow < x.End)
                        .ToHashSet();

        private async Task<TradingWindow> CreateTradingWindowAsync(
            int currencyPairId,
            int tradingWindowSettingId,
            int barrierCount,
            decimal barrierStep,
            TimeSpan duration)
        {
            var openingPrice = await this.currencyPairService.GetCurrencyRateAsync(currencyPairId);

            int startingIndex = -barrierCount / 2;

            decimal[] barriers = Enumerable.Range(startingIndex, barrierCount)
                .Select(barrierIndex => decimal.Round(openingPrice + barrierIndex * barrierStep, GlobalConstants.DecimalPlaces))
                .Reverse()
                .ToArray();

            var start = DateTime.UtcNow;

            var model = new TradingWindow
            {
                CurrencyPairId = currencyPairId,
                SettingId = tradingWindowSettingId,
                Barriers = barriers,
                Start = start,
                End = start.Add(duration)
            };

            await this.tradingWindowRepository.AddAsync(model);
            await this.tradingWindowRepository.SaveChangesAsync();

            return model;
        }
    }
}
