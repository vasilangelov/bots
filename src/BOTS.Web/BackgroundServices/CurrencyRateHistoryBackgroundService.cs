namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Services.Currencies;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Web.Models;
    using BOTS.Web.Hubs;
    using BOTS.Common;

    public class CurrencyRateHistoryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICurrencyRateHistoryProviderService currencyRateHistoryProvider;
        private readonly IHubContext<CurrencyHub> hubContext;

        public CurrencyRateHistoryBackgroundService(
            IServiceProvider serviceProvider,
            ICurrencyRateHistoryProviderService currencyRateHistoryProvider,
            IHubContext<CurrencyHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.currencyRateHistoryProvider = currencyRateHistoryProvider;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

                    var currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync(cancellationToken);

                    foreach (var currencyPairId in currencyPairIds)
                    {
                        var (fromCurrency, toCurrency) = await currencyPairService.GetCurrencyPairNamesAsync(currencyPairId, cancellationToken);

                        var model = await this.currencyRateHistoryProvider
                            .GetCurrencyRateHistoryAsync<CurrencyRateHistoryViewModel>(
                            fromCurrency,
                            toCurrency,
                            cancellationToken);

                        await this.hubContext.Clients.Group(currencyPairId.ToString()).SendAsync(
                            "AddCurrencyRateHistory",
                            model,
                            cancellationToken);
                    }
                }

                await Task.Delay(
                    GlobalConstants.CurrencyRateHistoryUpdateFrequency,
                    cancellationToken);
            }
        }
    }
}
