namespace BOTS.Web.BackgroundServices
{
    using System.Threading;
    using System.Threading.Tasks;

    using BOTS.Common;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.CurrencyRateStats;
    using BOTS.Web.Hubs;
    using BOTS.Web.Models.ViewModels;

    using Microsoft.AspNetCore.SignalR;

    public class CurrencyRateStatBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<TradingHub> hubContext;

        public CurrencyRateStatBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<TradingHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

                    var currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync();

                    await Parallel.ForEachAsync(
                            currencyPairIds,
                            async (id, ct) =>
                            {
                                using var newScope = this.serviceProvider.CreateAsyncScope();

                                var currencyRateStatService = newScope.ServiceProvider
                                        .GetRequiredService<ICurrencyRateStatService>();

                                var model = await currencyRateStatService
                                        .GetLatestStatAsync<CurrencyRateHistoryViewModel>(id);

                                await this.hubContext.Clients
                                    .Group(id.ToString())
                                    .SendAsync("AddCurrencyRateHistory", model, ct);
                            });
                }

                await Task.Delay(GlobalConstants.CurrencyRateStatUpdateFrequency, cancellationToken);
            }
        }
    }
}
