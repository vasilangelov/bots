namespace BOTS.Services
{
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;

    using BOTS.Common;
    using BOTS.Services.Models;

    public class CurrencyProviderService : ICurrencyProviderService
    {
        private const decimal precision = 1000000;
        private const int maxDeltaOffset = 10000;

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, decimal>> currencyCache = new();
        private static readonly Random rnd = new();

        private readonly IHttpClientFactory httpClientFactory;

        public CurrencyProviderService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> GetCurrencyRateAsync(string baseCurrency,
                                                        string convertCurrency,
                                                        CancellationToken cancellationToken = default)
        {
            bool baseExists = currencyCache.TryGetValue(baseCurrency, out var convertRates);

            if (!baseExists)
            {
                var rates = await this.FetchCurrencyRatesAsync(baseCurrency,
                                                                new string[] { convertCurrency },
                                                                cancellationToken);

                convertRates = new ConcurrentDictionary<string, decimal>(rates);

                bool success = currencyCache.TryAdd(baseCurrency, convertRates);

                if (!success)
                {
                    throw new Exception(string.Format("Could not set currency rate {0}/{1}",
                                                      baseCurrency,
                                                      convertCurrency));
                }

                return rates[convertCurrency];
            }

            bool convertExists = convertRates!.TryGetValue(convertCurrency, out var rate);

            if (!convertExists)
            {
                rate = await this.FetchCurrencyRateAsync(baseCurrency,
                                                         convertCurrency,
                                                         cancellationToken);

                bool success = currencyCache[baseCurrency].TryAdd(convertCurrency, rate);

                if (!success)
                {
                    throw new Exception(string.Format("Could not set currency rate {0}/{1}",
                                                      baseCurrency,
                                                      convertCurrency));
                }

                return rate;
            }

            return rate;
        }

        public async Task UpdateCurrencyRatesAsync(IDictionary<string, IEnumerable<string>> currencyPairs,
                                                   CancellationToken cancellationToken = default)
        {
            foreach (var baseCurrency in currencyPairs.Keys)
            {
                if (!currencyCache.ContainsKey(baseCurrency))
                {
                    var convertRates = await this.FetchCurrencyRatesAsync(baseCurrency,
                                                                          currencyPairs[baseCurrency],
                                                                          cancellationToken);

                    bool success = currencyCache.TryAdd(baseCurrency, new ConcurrentDictionary<string, decimal>(convertRates));
                }

                foreach (var convertCurrency in currencyPairs[baseCurrency])
                {
                    if (!currencyCache[baseCurrency].ContainsKey(convertCurrency))
                    {
                        decimal result = await this.FetchCurrencyRateAsync(baseCurrency, convertCurrency, cancellationToken);

                        bool success = currencyCache[baseCurrency].TryAdd(convertCurrency, result);
                    }

                    currencyCache[baseCurrency][convertCurrency] = CalculateUpdatedCurrencyRate(currencyCache[baseCurrency][convertCurrency]);
                }
            }
        }

        private async Task<decimal> FetchCurrencyRateAsync(string baseCurrency, string convertCurrency, CancellationToken cancellationToken = default)
        {
            var queryParams = string.Format("?base={0}&symbols={1}&places={2}", baseCurrency, convertCurrency, GlobalConstants.DecimalPlaces);

            using var httpClient = this.httpClientFactory.CreateClient("CurrencyAPI");

            var currencyInfo = await httpClient.GetFromJsonAsync<CurrencyInfo>(queryParams, cancellationToken);

            if (currencyInfo == null)
            {
                throw new ArgumentException(string.Format("Could not retrieve {0} to {1} currency info",
                                                        baseCurrency,
                                                        convertCurrency));
            }

            return currencyInfo.Rates[convertCurrency];
        }

        private async Task<IDictionary<string, decimal>> FetchCurrencyRatesAsync(
            string baseCurrency,
            IEnumerable<string> convertCurrencies,
            CancellationToken cancellationToken = default)
        {
            var queryParams = string.Format("?base={0}&symbols={1}&places={2}", baseCurrency, string.Join(",", convertCurrencies), GlobalConstants.DecimalPlaces);

            using var httpClient = this.httpClientFactory.CreateClient("CurrencyAPI");

            var currencyInfo = await httpClient.GetFromJsonAsync<CurrencyInfo>(queryParams, cancellationToken);

            if (currencyInfo == null)
            {
                throw new ArgumentException(string.Format("Could not retrieve {0} to {1} currency info",
                                                        baseCurrency,
                                                        string.Join(",", convertCurrencies)));
            }

            return currencyInfo.Rates;
        }

        private static decimal CalculateUpdatedCurrencyRate(decimal value)
        {
            int sign = rnd.Next(-1, 2);
            decimal delta = rnd.Next(maxDeltaOffset) / precision;

            return value + sign * delta;
        }
    }
}
