namespace BOTS.Services.Currencies
{
    using AutoMapper;
    using System.Collections.Concurrent;

    using BOTS.Services.Currencies.Models;
    using BOTS.Services.Infrastructure.Extensions;

    public class CurrencyRateStatGeneratorService : ICurrencyRateStatProviderService
    {
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
            string toCurrency,
            CancellationToken cancellationToken = default)
        {
            var currencyRate = await this.currencyRateProviderService.GetCurrencyRateAsync(fromCurrency, toCurrency, cancellationToken);

            Random rnd = new();

            var openValue = GenerateCurrencyRate(rnd, currencyRate);
            var highValue = Math.Max(openValue, currencyRate) + rnd.NextDecimal(0.0005m, 0.0001m);
            var lowValue = Math.Min(openValue, currencyRate) - rnd.NextDecimal(0.0005m, 0.0001m);

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
            TimeSpan interval,
            CancellationToken cancellationToken = default)
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
            TimeSpan interval,
            CancellationToken cancellationToken = default)
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
                        toCurrency,
                        cancellationToken);
                }

                decimal openValue = lastValue.Value;
                decimal closeValue = GenerateCurrencyRate(rnd, openValue);

                lastValue = closeValue;

                decimal highValue = Math.Max(openValue, closeValue) + rnd.NextDecimal(0.0005m, 0.0001m);
                decimal lowValue = Math.Min(openValue, closeValue) - rnd.NextDecimal(0.0005m, 0.0001m);

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

        // TODO: merge logic with currencygenerator
        private const decimal precision = 0.000001m;
        private const int maxDeltaOffset = 500;

        private static decimal GenerateCurrencyRate(Random rnd, decimal value)
        {
            int sign = rnd.Next(-1, 2);
            decimal delta = rnd.Next(maxDeltaOffset) * precision;

            return value + sign * delta;
        }
    }
}
