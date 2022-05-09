namespace BOTS.Services.Currencies
{
    using AutoMapper;
    using System.Collections.Concurrent;

    using BOTS.Services.Currencies.Models;
    using BOTS.Services.Infrastructure.Extensions;

    using static BOTS.Services.Currencies.CurrencyGenerator;
    using static BOTS.Common.GlobalConstants;

    public class CurrencyRateStatGeneratorService : ICurrencyRateStatProviderService
    {
        private const decimal MaxHighLowOffset = 0.0005m;
        private const decimal MinHighLowOffset = 0.0001m;

        private static readonly ConcurrentDictionary<(string, string), DateTime> lastUpdated = new();

        private readonly ICurrencyRateProviderService currencyRateProviderService;
        private readonly IMapper mapper;

        public CurrencyRateStatGeneratorService(
            ICurrencyRateProviderService currencyRateProviderService,
            IMapper mapper)
        {
            this.currencyRateProviderService = currencyRateProviderService;
            this.mapper = mapper;
        }

        public async Task<T> GetLatestCurrencyRateStatAsync<T>(
            string fromCurrency,
            string toCurrency)
        {
            var currencyRate = await this.currencyRateProviderService.GetCurrencyRateAsync(fromCurrency, toCurrency);

            Random rnd = new();

            var openValue = GenerateCurrencyRate(rnd, currencyRate, MinCurrencyRateOffset, MaxCurrencyRateOffset);
            var highValue = Math.Max(openValue, currencyRate) + rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);
            var lowValue = Math.Min(openValue, currencyRate) - rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);

            DateTime end = DateTime.UtcNow;

            lastUpdated[(fromCurrency, toCurrency)] = end;

            var result = new CurrencyRateHistory
            {
                Time = end,
                Open = openValue,
                High = highValue,
                Low = lowValue,
                Close = currencyRate,
            };

            return this.mapper.Map<T>(result);
        }

        public async Task<IEnumerable<T>> GetLatestCurrencyRateStatsAsync<T>(
            string fromCurrency,
            string toCurrency,
            DateTime start,
            TimeSpan interval)
        {
            bool success = lastUpdated.TryGetValue((fromCurrency, toCurrency), out var end);

            return await this.GetCurrencyRateStatsAsync<T>(
                fromCurrency,
                toCurrency,
                start,
                end,
                interval);
        }

        public async Task<IEnumerable<T>> GetCurrencyRateStatsAsync<T>(
            string fromCurrency,
            string toCurrency,
            DateTime start,
            DateTime end,
            TimeSpan interval)
        {
            Random rnd = new(0);

            var currencyRateHistory = new Stack<CurrencyRateHistory>();

            decimal? lastValue = null;

            for (DateTime time = end; time >= start; time = time.Subtract(interval))
            {
                if (!lastValue.HasValue)
                {
                    lastValue = await this.currencyRateProviderService.GetCurrencyRateAsync(
                        fromCurrency,
                        toCurrency);
                }

                decimal openValue = lastValue.Value;
                decimal closeValue = GenerateCurrencyRate(rnd, openValue, MinCurrencyRateOffset, MaxCurrencyRateOffset);

                lastValue = closeValue;

                decimal highValue = Math.Max(openValue, closeValue) + rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);
                decimal lowValue = Math.Min(openValue, closeValue) - rnd.NextDecimal(MaxHighLowOffset, MinHighLowOffset);

                currencyRateHistory.Push(new CurrencyRateHistory
                {
                    Time = time,
                    Open = openValue,
                    High = highValue,
                    Low = lowValue,
                    Close = closeValue,
                });
            }

            return this.mapper.Map<IEnumerable<T>>(currencyRateHistory);
        }
    }
}
