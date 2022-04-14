namespace BOTS.Services
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System.Net.Http.Json;
    using System.Text.Json;

    using BOTS.Data.Models;
    using BOTS.Services.Models;

    public class CurrencyProviderService : ICurrencyProviderService
    {
        private const decimal precision = 1000000;
        private const int maxDeltaOffset = 10;

        private readonly string queryParams;
        private readonly JsonSerializerOptions jsonOptions;
        private readonly IHttpClientFactory httpClientFactory;

        private CurrencyInfo? currencyInfo;

        public CurrencyProviderService(IOptions<JsonSerializerOptions> jsonOptions, IHttpClientFactory httpClientFactory, IServiceProvider serviceProvider)
        {
            this.jsonOptions = jsonOptions.Value;
            this.httpClientFactory = httpClientFactory;

            using (var scope = serviceProvider.CreateScope())
            {
                var currencyRepository = scope.ServiceProvider.GetRequiredService<IRepository<CurrencyPair>>();

                IEnumerable<string> currencies = currencyRepository
                                                    .AllAsNotracking()
                                                    .Where(x => x.Display)
                                                    .Select(x => x.Left.Name)
                                                    .Concat(currencyRepository
                                                        .AllAsNotracking()
                                                        .Where(x => x.Display)
                                                        .Select(x => x.Right.Name))
                                                    .Distinct()
                                                    .ToArray();

                this.queryParams = $"?base=USD&symbols={string.Join(",", currencies)}&places=10";
            }
        }

        public decimal GetCurrencyRate(string left, string right)
        {
            if (this.currencyInfo is null)
            {
                throw new InvalidOperationException("Could not obtain currency info");
            }

            decimal leftValue = this.currencyInfo.Rates[left];

            if (this.currencyInfo.Base == right)
            {
                return 1 / leftValue;
            }

            decimal rightValue = this.currencyInfo.Rates[right];

            return rightValue / leftValue;
        }

        public async Task UpdateCurrencyInfoAsync(CancellationToken cancellationToken = default)
        {
            if (this.currencyInfo == default)
            {
                using var httpClient = this.httpClientFactory.CreateClient("CurrencyApi");

                this.currencyInfo = await httpClient.GetFromJsonAsync<CurrencyInfo>(this.queryParams, this.jsonOptions, cancellationToken);
            }
            else
            {
                Random rnd = new();

                foreach (var currency in this.currencyInfo.Rates.Keys)
                {
                    int sign = rnd.Next(-1, 2);
                    decimal delta = rnd.Next(maxDeltaOffset) / precision;

                    this.currencyInfo.Rates[currency] += sign * delta;
                }
            }
        }
    }
}
