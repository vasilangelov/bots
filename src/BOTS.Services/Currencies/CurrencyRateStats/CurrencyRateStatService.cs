namespace BOTS.Services.CurrencyRateStats
{
    using System.Collections.Concurrent;

    using AutoMapper;

    using BOTS.Services.Common;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.CurrencyRateStats.Models;
    using BOTS.Services.Infrastructure.Extensions;

    using static BOTS.Common.GlobalConstants;
    using static BOTS.Services.Currencies.Common.CurrencyGenerator;

    [TransientService]
    public class CurrencyRateStatService : ICurrencyRateStatService
    {
        private const decimal MaxHighLowOffset = 0.0005m;
        private const decimal MinHighLowOffset = 0.0001m;

        private static readonly ConcurrentDictionary<int, DateTime> lastUpdated = new();

        private readonly ICurrencyRateProviderService currencyRateProviderService;
        private readonly IMapper mapper;

        public CurrencyRateStatService(
            ICurrencyRateProviderService currencyRateProviderService,
            IMapper mapper)
        {
            this.currencyRateProviderService = currencyRateProviderService;
            this.mapper = mapper;
        }

        public async Task<T> GetLatestStatAsync<T>(int currencyPairId)
        {
            var currencyRate = await this.currencyRateProviderService.GetCurrencyRateAsync(currencyPairId);

            Random rnd = new();

            var openValue = GenerateCurrencyRate(rnd, currencyRate, MinCurrencyRateOffset, MaxCurrencyRateOffset);
            var highValue = Math.Max(openValue, currencyRate) + rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);
            var lowValue = Math.Min(openValue, currencyRate) - rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);

            DateTime end = DateTime.UtcNow;

            lastUpdated[currencyPairId] = end;

            var result = new CurrencyRateStat
            {
                Time = end,
                Open = openValue,
                High = highValue,
                Low = lowValue,
                Close = currencyRate,
            };

            return this.mapper.Map<T>(result);
        }

        public async Task<IEnumerable<T>> GetStatsAsync<T>(
            int currencyPairId,
            DateTime start,
            TimeSpan interval)
        {
            lastUpdated.TryGetValue(currencyPairId, out var end);

            return await this.GetStatsForIntervalAsync<T>(
                currencyPairId,
                start,
                end,
                interval);
        }

        private async Task<IEnumerable<T>> GetStatsForIntervalAsync<T>(
            int currencyPairId,
            DateTime start,
            DateTime end,
            TimeSpan interval)
        {
            Random rnd = new(0);

            var currencyRateStatHistory = new Stack<CurrencyRateStat>();

            decimal? lastValue = null;

            for (DateTime time = end; time >= start; time = time.Subtract(interval))
            {
                if (!lastValue.HasValue)
                {
                    lastValue = await this.currencyRateProviderService.GetCurrencyRateAsync(currencyPairId);
                }

                decimal openValue = lastValue.Value;
                decimal closeValue = GenerateCurrencyRate(rnd, openValue, MinCurrencyRateOffset, MaxCurrencyRateOffset);

                lastValue = closeValue;

                decimal highValue = Math.Max(openValue, closeValue) + rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);
                decimal lowValue = Math.Min(openValue, closeValue) - rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);

                currencyRateStatHistory.Push(new CurrencyRateStat
                {
                    Time = time,
                    Open = openValue,
                    High = highValue,
                    Low = lowValue,
                    Close = closeValue,
                });
            }

            return this.mapper.Map<IEnumerable<T>>(currencyRateStatHistory);
        }
    }
}
