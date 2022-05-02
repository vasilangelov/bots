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

    public class CurrencyRateHistoryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICurrencyRateStatProviderService currencyRateHistoryProvider;
        private readonly IHubContext<CurrencyHub> hubContext;

        public CurrencyRateHistoryBackgroundService(
            IServiceProvider serviceProvider,
            ICurrencyRateStatProviderService currencyRateHistoryProvider,
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

                    var currencyPairs = await currencyPairService.GetActiveCurrencyPairsAsync<CurrencyPairViewModel>(cancellationToken);

                    await Parallel.ForEachAsync(
                        currencyPairs,
                        async (p, ct) =>
                        {
                            var model = await this.currencyRateHistoryProvider
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
