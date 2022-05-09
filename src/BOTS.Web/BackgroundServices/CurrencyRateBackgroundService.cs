namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Common;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Web.Hubs;

    public class CurrencyRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<CurrencyHub> currencyHub;

        public CurrencyRateBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<CurrencyHub> currencyHub)
        {
            this.serviceProvider = serviceProvider;
            this.currencyHub = currencyHub;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

                    // TODO: maybe some kind of caching...
                    var currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync();

                    foreach (var currencyPairId in currencyPairIds)
                    {
                        decimal currencyRate = await currencyPairService.GetCurrencyRateAsync(currencyPairId);

                        await this.currencyHub.Clients
                                    .Group(currencyPairId.ToString())
                                    .SendAsync("UpdateCurrencyRate", currencyRate, cancellationToken);
                    }
                }

                await Task.Delay(GlobalConstants.CurrencyValueUpdateFrequency, cancellationToken);
            }
        }
    }
}
