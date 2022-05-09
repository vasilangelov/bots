namespace BOTS.Services.Currencies
{
    using System.Net.Http.Json;

    using BOTS.Common;
    using BOTS.Services.Currencies.Models;

    public class ThirdPartyCurrencyRateProviderService : ICurrencyRateProviderService
    {
        private const string ApiQueryString = "?base={0}&symbols={1}&places={2}";
        private const string UnsuccessfulRetrivalExceptionMessage = "Could not retrieve currency info {0} to {1} ";

        private readonly HttpClient httpClient;

        public ThirdPartyCurrencyRateProviderService(HttpClient httpClient)
        {
            this.httpClient = httpClient;

            httpClient.BaseAddress = new Uri("https://api.exchangerate.host/latest");
        }

        public async Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency)
        {
            var queryParams = string.Format(
                ApiQueryString,
                fromCurrency,
                toCurrency,
                GlobalConstants.DecimalPlaces);

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
            IEnumerable<(string FromCurrency, string ToCurrency)> currencyPairs,
            CancellationToken cancellationToken = default)
        {
            string baseCurrency = currencyPairs.First().FromCurrency;

            var convertCurrencies = currencyPairs
                .SelectMany(x => new[] { x.FromCurrency, x.ToCurrency })
                .Distinct()
                .ToArray();

            var queryParams = string.Format(
                ApiQueryString,
                baseCurrency,
                string.Join(",", convertCurrencies),
                GlobalConstants.DecimalPlaces);

            CurrencyRateInfo? currencyRateInfo = await httpClient.GetFromJsonAsync<CurrencyRateInfo>(
                queryParams,
                cancellationToken);

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

        public Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime)
        {
            throw new NotImplementedException();
        }
    }
}
