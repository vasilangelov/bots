namespace BOTS.Services.Currencies
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using static BOTS.Services.Currencies.CurrencyGenerator;
    using static BOTS.Common.GlobalConstants;

    public class CurrencyGeneratorService : ICurrencyRateProviderService, ICurrencyRateGeneratorService
    {
        private static readonly Random rnd = new();
        private static readonly ConcurrentDictionary<(string, string), decimal> currencyRates = new();

        private readonly IServiceProvider serviceProvider;

        public CurrencyGeneratorService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task SeedInitialCurrencyRatesAsync(
            IEnumerable<(string, string)> currencyPairs)
        {
            using var scope = this.serviceProvider.CreateScope();

            var currencyRateProviderService = scope.ServiceProvider.GetRequiredService<ThirdPartyCurrencyRateProviderService>();

            var seededCurrencyRates = await currencyRateProviderService.GetCurrencyRatesAsync(currencyPairs);

            foreach (var currencyRate in seededCurrencyRates.Keys)
            {
                currencyRates.TryAdd(currencyRate, seededCurrencyRates[currencyRate]);
            }
        }

        public async Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency)
        {
            if (!currencyRates.ContainsKey((fromCurrency, toCurrency)))
            {
                using var scope = this.serviceProvider.CreateScope();

                ICurrencyRateProviderService currencyRateProvider = scope.ServiceProvider.GetRequiredService<ThirdPartyCurrencyRateProviderService>();

                decimal currencyRate = await currencyRateProvider.GetCurrencyRateAsync(
                    fromCurrency,
                    toCurrency);

                currencyRates.TryAdd((fromCurrency, toCurrency), currencyRate);
            }

            return currencyRates[(fromCurrency, toCurrency)];
        }

        public async Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime)
        {
            decimal currencyRate = await this.GetCurrencyRateAsync(fromCurrency, toCurrency);

            return GenerateCurrencyRate(rnd, currencyRate, MinCurrencyRateOffset, MaxCurrencyRateOffset);
        }

        public void UpdateCurrencyRates()
        {
            foreach (var currencyPair in currencyRates.Keys)
            {
                currencyRates[currencyPair] = GenerateCurrencyRate(rnd, currencyRates[currencyPair], MinCurrencyRateOffset, MaxCurrencyRateOffset);
            }
        }
    }
}
