namespace BOTS.Services
{
    using Microsoft.Extensions.Options;
    using System.Net.Http.Json;
    using System.Text.Json;

    using Models;

    public class CurrencyProviderService : ICurrencyProviderService
    {
        private const decimal precision = 1000000;
        private const int maxDeltaOffset = 11;

        private readonly CurrencyProviderOptions options;
        private readonly string queryParams;
        private readonly JsonSerializerOptions jsonOptions;
        private readonly IHttpClientFactory httpClientFactory;
        private CurrencyInfo? instance;

        public CurrencyProviderService(IOptions<CurrencyProviderOptions> options, IOptions<JsonSerializerOptions> jsonOptions, IHttpClientFactory httpClientFactory)
        {
            this.options = options.Value;
            this.jsonOptions = jsonOptions.Value;
            this.httpClientFactory = httpClientFactory;

            string currencies = string.Join(",",
                                    Enum.GetValues<Currency>()
                                        .Where(c => c != this.options.Base)
                                        .ToArray());

            this.queryParams = $"?base={this.options.Base}&symbols={currencies}&places=6";
        }

        public CurrencyInfo? GetCurrencyInfo()
            => this.instance;

        public async Task UpdateCurrencyInfoAsync(CancellationToken cancellationToken)
        {
            if (this.instance == default)
            {
                using var client = this.httpClientFactory.CreateClient("CurrencyApi");

                this.instance = await client.GetFromJsonAsync<CurrencyInfo>(queryParams, this.jsonOptions, cancellationToken);
            }
            else
            {
                Random rnd = new();

                foreach (var currency in this.instance.Rates.Keys)
                {
                    int sign = rnd.Next(-1, 2);
                    decimal delta = rnd.Next(maxDeltaOffset) / precision;

                    this.instance.Rates[currency] += sign * delta;
                }
            }
        }
    }
}
