namespace BOTS.Services.Currencies.CurrencyRates
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using BOTS.Services.Common;

    using static BOTS.Services.Currencies.Common.CurrencyGenerator;
    using static BOTS.Common.GlobalConstants;

    [TransientService]
    public class CurrencyGeneratorService : ICurrencyRateProviderService, ICurrencyRateGeneratorService
    {
        private static readonly Random rnd = new();
        private static readonly ConcurrentDictionary<(string, string), decimal> currencyRates = new();

        private readonly ICurrencyPairService currencyPairService;
        private readonly IThirdPartyCurrencyRateProviderService thirdPartyCurrencyRateProviderService;

        public CurrencyGeneratorService(
            ICurrencyPairService currencyPairService,
            IThirdPartyCurrencyRateProviderService thirdPartyCurrencyRateProviderService)
        {
            this.currencyPairService = currencyPairService;
            this.thirdPartyCurrencyRateProviderService = thirdPartyCurrencyRateProviderService;
        }

        public async Task<decimal> GetCurrencyRateAsync(int currencyPairId)
        {
            var (fromCurrency, toCurrency) =
                await this.currencyPairService.GetCurrencyPairNamesAsync(currencyPairId);

            return await this.GetCurrencyRateAsync(
                fromCurrency,
                toCurrency);
        }

        public async Task<decimal> GetPastCurrencyRateAsync(int currencyPairId, DateTime dateTime)
        {
            var (fromCurrency, toCurrency) =
                await this.currencyPairService.GetCurrencyPairNamesAsync(currencyPairId);

            return await this.GetPastCurrencyRateAsync(
                fromCurrency,
                toCurrency,
                dateTime);
        }

        public async Task SeedInitialCurrencyRatesAsync()
        {
            var activeCurrencies =
                await currencyPairService.GetAllActiveCurrencyPairNamesAsync();

            await this.SeedInitialCurrencyRatesAsync(activeCurrencies);
        }

        public void UpdateCurrencyRates()
        {
            foreach (var currencyPair in currencyRates.Keys)
            {
                currencyRates[currencyPair] = decimal.Round(GenerateCurrencyRate(rnd, currencyRates[currencyPair], MinCurrencyRateOffset, MaxCurrencyRateOffset), DecimalPlacePrecision);
            }
        }

        private async Task SeedInitialCurrencyRatesAsync(
            IEnumerable<(string, string)> currencyPairs)
        {
            var seededCurrencyRates =
                await this.thirdPartyCurrencyRateProviderService.GetCurrencyRatesAsync(currencyPairs);

            foreach (var currencyRate in seededCurrencyRates.Keys)
            {
                currencyRates.TryAdd(currencyRate, seededCurrencyRates[currencyRate]);
            }
        }

        private async Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency)
        {
            if (!currencyRates.ContainsKey((fromCurrency, toCurrency)))
            {
                decimal currencyRate =
                    await this.thirdPartyCurrencyRateProviderService.GetCurrencyRateAsync(
                        fromCurrency,
                        toCurrency);

                currencyRates.TryAdd((fromCurrency, toCurrency), currencyRate);
            }

            return currencyRates[(fromCurrency, toCurrency)];
        }

        private async Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime)
        {
            if (dateTime.Date == DateTime.UtcNow.Date)
            {
                return await this.GetCurrencyRateAsync(fromCurrency, toCurrency);
            }

            decimal currencyRate =
                await this.thirdPartyCurrencyRateProviderService.GetPastCurrencyRateAsync(
                    fromCurrency,
                    toCurrency,
                    dateTime);

            return GenerateCurrencyRate(rnd, currencyRate, MinCurrencyRateOffset, MaxCurrencyRateOffset);
        }
    }
}
