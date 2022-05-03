namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Services.Currencies;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Web.Models;
    using BOTS.Web.Hubs;
    using BOTS.Common;
    using System.Threading.Tasks;
    using System.Threading;

    public class CurrencyRateStatBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICurrencyRateStatProviderService currencyRateStatProvider;
        private readonly IHubContext<CurrencyHub> hubContext;

        public CurrencyRateStatBackgroundService(
            IServiceProvider serviceProvider,
            ICurrencyRateStatProviderService currencyRateStatProvider,
            IHubContext<CurrencyHub> hubContext)
        {
            this.serviceProvider = serviceProvider;
            this.currencyRateStatProvider = currencyRateStatProvider;
            this.hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

                    var currencyPairs = await currencyPairService.GetActiveCurrencyPairsAsync<CurrencyPairViewModel>(cancellationToken);

                    await Parallel.ForEachAsync(
                        currencyPairs,
                        async (p, ct) =>
                        {
                            var model = await this.currencyRateStatProvider
                                .GetLatestCurrencyRateStatAsync<CurrencyRateHistoryViewModel>(
                                    p.LeftName,
                                    p.RightName,
                                    cancellationToken);

                            await this.hubContext.Clients
                                .Group(p.Id.ToString())
                                .SendAsync("AddCurrencyRateHistory", model, ct);
                        });
                }

                await Task.Delay(GlobalConstants.CurrencyRateStatUpdateFrequency, cancellationToken);
            }
        }
    }
}
