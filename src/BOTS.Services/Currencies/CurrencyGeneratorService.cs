namespace BOTS.Services.Currencies
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class CurrencyGeneratorService : ICurrencyRateProviderService, ICurrencyRateGeneratorService
    {
        private static readonly ConcurrentDictionary<(string, string), decimal> currencyRates = new();

        private readonly IServiceProvider serviceProvider;

        public CurrencyGeneratorService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task SeedInitialCurrencyRatesAsync(
            IEnumerable<(string, string)> currencyPairs,
            CancellationToken cancellationToken = default)
        {
            using var scope = this.serviceProvider.CreateScope();

            var currencyRateProviderService = scope.ServiceProvider.GetRequiredService<ThirdPartyCurrencyRateProviderService>();

            var seededCurrencyRates = await currencyRateProviderService.GetCurrencyRatesAsync(currencyPairs, cancellationToken);

            foreach (var currencyRate in seededCurrencyRates.Keys)
            {
                currencyRates.TryAdd(currencyRate, seededCurrencyRates[currencyRate]);
            }
        }

        public async Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            CancellationToken cancellationToken = default)
        {
            if (!currencyRates.ContainsKey((fromCurrency, toCurrency)))
            {
                using var scope = this.serviceProvider.CreateScope();

                var currencyRateProvider = scope.ServiceProvider.GetRequiredService<ThirdPartyCurrencyRateProviderService>();

                decimal currencyRate = await currencyRateProvider.GetCurrencyRateAsync(
                    fromCurrency,
                    toCurrency,
                    cancellationToken);

                currencyRates.TryAdd((fromCurrency, toCurrency), currencyRate);
            }

            return currencyRates[(fromCurrency, toCurrency)];
        }

        public void UpdateCurrencyRates()
        {
            foreach (var currencyPair in currencyRates.Keys)
            {
                currencyRates[currencyPair] = GenerateCurrencyRate(currencyRates[currencyPair]);
            }
        }

        // TODO: extract constants...
        private const decimal precision = 1000000;
        private const int maxDeltaOffset = 1000;
        private static readonly Random rnd = new();

        private static decimal GenerateCurrencyRate(decimal value)
        {
            int sign = rnd.Next(-1, 2);
            decimal delta = rnd.Next(maxDeltaOffset) / precision;

            return value + sign * delta;
        }
    }
}
