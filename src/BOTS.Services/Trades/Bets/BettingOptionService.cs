namespace BOTS.Services.Trades.Bets
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Data.Infrastructure.Repositories;
    using BOTS.Data.Models;
    using BOTS.Services.Common;
    using BOTS.Services.Currencies.CurrencyRates;

    using static BOTS.Services.Trades.Bets.BarrierActions;

    [TransientService]
    public class BettingOptionService : IBettingOptionService
    {
        private readonly IRepository<BettingOption> bettingOptionRepository;
        private readonly ICurrencyRateProviderService currencyRateProviderService;
        private readonly IMapper mapper;

        public BettingOptionService(
            IRepository<BettingOption> bettingOptionRepository,
            ICurrencyRateProviderService currencyRateProviderService,
            IMapper mapper)
        {
            this.bettingOptionRepository = bettingOptionRepository;
            this.currencyRateProviderService = currencyRateProviderService;
            this.mapper = mapper;
        }

        public async Task BulkCreateBettingOptionsAsync(
            Guid tradingWindowId,
            IEnumerable<BettingOptionPreset> bettingOptionPresets)
        {
            foreach (var bettingOptionPreset in bettingOptionPresets)
            {
                decimal currencyRate = await this.currencyRateProviderService
                                        .GetCurrencyRateAsync(bettingOptionPreset.CurrencyPairId);

                decimal[] barriers = GenerateBarriers(
                    bettingOptionPreset.BarrierCount,
                    currencyRate,
                    bettingOptionPreset.BarrierStep);

                await this.bettingOptionRepository.AddAsync(new BettingOption
                {
                    Barriers = barriers,
                    BarrierStep = bettingOptionPreset.BarrierStep,
                    CurrencyPairId = bettingOptionPreset.CurrencyPairId,
                    TradingWindowId = tradingWindowId,
                });
            }

            await this.bettingOptionRepository.SaveChangesAsync();
        }

        public async Task SetBettingOptionsClosingValueAsync(Guid tradingWindowId, DateTime windowEnd)
        {
            var bettingOptions = await this.GetBettingOptionInfoAsync(tradingWindowId);

            foreach (var bettingOption in bettingOptions)
            {
                decimal closingValue =
                    await this.currencyRateProviderService.GetPastCurrencyRateAsync(
                        bettingOption.CurrencyPairId,
                        windowEnd);

                this.SetBettingOptionClosingValueAsync(bettingOption.Id, closingValue);
            }

            await this.bettingOptionRepository.SaveChangesAsync();
        }

        public async Task<bool> IsBettingOptionActiveAsync(Guid bettingOptionId)
            => await this.bettingOptionRepository
                            .AllAsNoTracking()
                            .AnyAsync(x => x.Id == bettingOptionId &&
                                           x.TradingWindow.End > DateTime.UtcNow);

        public async Task<DateTime> GetBettingOptionEndAsync(Guid bettingOptionId)
            => await this.bettingOptionRepository
                            .AllAsNoTracking()
                            .Where(x => x.Id == bettingOptionId)
                            .Select(x => x.TradingWindow.End)
                            .FirstAsync();

        public async Task<IEnumerable<T>> GetActiveBettingOptionsForCurrencyPairAsync<T>(
            int currencyPairId)
            => await this.bettingOptionRepository
                        .AllAsNoTracking()
                        .Where(x => x.CurrencyPairId == currencyPairId &&
                                    x.TradingWindow.End > DateTime.UtcNow)
                        .OrderBy(x => x.TradingWindow.Duration)
                        .ProjectTo<T>(this.mapper.ConfigurationProvider)
                        .ToArrayAsync();

        public async Task<T> GetBettingOptionAsync<T>(Guid bettingOptionId)
            => await this.bettingOptionRepository
                         .AllAsNoTracking()
                         .Where(x => x.Id == bettingOptionId)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .FirstAsync();

        public async Task<IEnumerable<T>> GetAllActiveBettingOptionsAsync<T>()
            => await this.bettingOptionRepository
                         .AllAsNoTracking()
                         .Where(x => !x.TradingWindow.IsClosed)
                         .ProjectTo<T>(this.mapper.ConfigurationProvider)
                         .ToArrayAsync();
        private async Task<IEnumerable<BettingOptionInfo>> GetBettingOptionInfoAsync(Guid tradingWindowId)
            => await this.bettingOptionRepository
                    .AllAsNoTracking()
                    .Where(x => x.TradingWindowId == tradingWindowId)
                    .Select(x => new BettingOptionInfo(x.Id, x.CurrencyPairId))
                    .ToArrayAsync();

        private record class BettingOptionInfo(Guid Id, int CurrencyPairId);

        private void SetBettingOptionClosingValueAsync(Guid bettingOptionId,
                                                       decimal closingValue)
        {
            var updatedBettingOption = new BettingOption
            {
                Id = bettingOptionId,
                CloseValue = closingValue,
            };

            this.bettingOptionRepository.Patch(updatedBettingOption, x => x.CloseValue!);
        }
    }
}
