namespace BOTS.Services.Currencies.CurrencyRates
{
    using System.Net.Http.Json;

    using BOTS.Common;
    using BOTS.Services.Common;
    using BOTS.Services.CurrencyRates.Models;

    [TransientService]
    public class ThirdPartyCurrencyRateProviderService : IThirdPartyCurrencyRateProviderService
    {
        private const string CurrentCurrencyRateQueryString = "?base={0}&symbols={1}&places={2}";
        private const string HistoryCurrencyRateQueryString = "?base={0}&symbols={1}&places={2}&date={3:yyyy-MM-dd}";
        private const string UnsuccessfulRetrivalExceptionMessage = "Could not retrieve currency info {0} to {1} ";

        private static readonly Uri baseAddres = new("https://api.exchangerate.host/latest");

        private readonly HttpClient httpClient;

        public ThirdPartyCurrencyRateProviderService(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            httpClient.BaseAddress = baseAddres;
        }

        public async Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency)
        {
            var queryParams = string.Format(
                CurrentCurrencyRateQueryString,
                fromCurrency,
                toCurrency,
                GlobalConstants.DecimalPlacePrecision);

            CurrencyRateInfo? currencyRateInfo =
                await httpClient.GetFromJsonAsync<CurrencyRateInfo>(queryParams);

            if (currencyRateInfo is null)
            {
                throw new InvalidOperationException(string.Format(
                    UnsuccessfulRetrivalExceptionMessage,
                    fromCurrency,
                    toCurrency));
            }

            return currencyRateInfo.Rates[toCurrency];
        }

        public async Task<IDictionary<(string FromCurrency, string ToCurrency), decimal>> GetCurrencyRatesAsync(
            IEnumerable<(string FromCurrency, string ToCurrency)> currencyPairs)
        {
            string baseCurrency = currencyPairs.First().FromCurrency;

            var convertCurrencies = currencyPairs
                .SelectMany(x => new[] { x.FromCurrency, x.ToCurrency })
                .Distinct()
                .ToArray();

            var queryParams = string.Format(
                CurrentCurrencyRateQueryString,
                baseCurrency,
                string.Join(",", convertCurrencies),
                GlobalConstants.DecimalPlacePrecision);

            CurrencyRateInfo? currencyRateInfo = await httpClient.GetFromJsonAsync<CurrencyRateInfo>(
                queryParams);

            if (currencyRateInfo is null)
            {
                throw new InvalidOperationException(string.Format(
                    UnsuccessfulRetrivalExceptionMessage,
                    baseCurrency,
                    string.Join(",", convertCurrencies)));
            }

            var currencyRates = new Dictionary<(string, string), decimal>();

            foreach (var currencyPair in currencyPairs)
            {
                decimal currencyRate;

                var fromCurrency = currencyRateInfo.Rates[currencyPair.FromCurrency];

                if (currencyRateInfo.Base == currencyPair.ToCurrency)
                {
                    currencyRate = 1 / fromCurrency;
                }
                else
                {
                    var toCurrency = currencyRateInfo.Rates[currencyPair.ToCurrency];

                    currencyRate = toCurrency / fromCurrency;
                }

                currencyRates.Add(currencyPair, currencyRate);
            }

            return currencyRates;
        }

        public async Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime)
        {
            var queryParams = string.Format(
                HistoryCurrencyRateQueryString,
                fromCurrency,
                toCurrency,
                GlobalConstants.DecimalPlacePrecision,
                dateTime);

            CurrencyRateInfo? currencyRateInfo =
                await httpClient.GetFromJsonAsync<CurrencyRateInfo>(queryParams);

            if (currencyRateInfo is null)
            {
                throw new InvalidOperationException(string.Format(
                    UnsuccessfulRetrivalExceptionMessage,
                    fromCurrency,
                    toCurrency));
            }

            return currencyRateInfo.Rates[toCurrency];
        }
    }
}
