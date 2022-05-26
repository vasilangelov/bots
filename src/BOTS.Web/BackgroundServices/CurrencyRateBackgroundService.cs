namespace BOTS.Web.BackgroundServices
{
    using BOTS.Common;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Web.Hubs.Trading;

    using Microsoft.AspNetCore.SignalR;

    public class CurrencyRateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<TradingHub> currencyHub;

        public CurrencyRateBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<TradingHub> currencyHub)
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

                    var currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync();

                    await Parallel.ForEachAsync(currencyPairIds, async (id, ct) =>
                    {
                        using var newScope = this.serviceProvider.CreateAsyncScope();

                        var currencyRateProviderService = newScope.ServiceProvider.GetRequiredService<ICurrencyRateProviderService>();

                        decimal currencyRate =
                            await currencyRateProviderService.GetCurrencyRateAsync(id);

                        await this.currencyHub.Clients
                                    .Group(id.ToString())
                                    .SendAsync("UpdateCurrencyRate", currencyRate, ct);
                    });
                }

                await Task.Delay(GlobalConstants.CurrencyValueUpdateFrequency, cancellationToken);
            }
        }
    }
}
