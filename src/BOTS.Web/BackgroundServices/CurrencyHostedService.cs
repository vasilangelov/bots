namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Services;
    using BOTS.Web.Hubs;
    using BOTS.Common;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Data.TradingWindows;
    using BOTS.Web.Models;

    public class CurrencyHostedService : BackgroundService
    {
        private readonly IHubContext<CurrencyHub> hubContext;
        private readonly IServiceProvider serviceProvider;

        public CurrencyHostedService(IHubContext<CurrencyHub> hubContext, IServiceProvider serviceProvider)
        {
            this.hubContext = hubContext;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

                    var activeCurrencyRates = await currencyPairService.GetActiveCurrencyPairNamesAsync(cancellationToken);

                    var currencyProviderService = scope.ServiceProvider.GetRequiredService<ICurrencyProviderService>();

                    await currencyProviderService.UpdateCurrencyRatesAsync(activeCurrencyRates, cancellationToken);

                    IEnumerable<int> currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync(cancellationToken);

                    var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

                    await tradingWindowService.EnsureAllTradingWindowsActiveAsync(currencyPairIds, cancellationToken);

                    var now = DateTime.UtcNow;

                    foreach (var currencyPairId in currencyPairIds)
                    {
                        decimal currencyRate = await currencyPairService.GetCurrencyRateAsync(currencyPairId, cancellationToken);

                        await this.hubContext.Clients
                                    .Group(currencyPairId.ToString())
                                    .SendAsync("UpdateCurrencyRate", currencyRate, cancellationToken);

                        var tradingWindows = await tradingWindowService.GetActiveTradingWindowsByCurrencyPairAsync(currencyPairId, x => new
                        {
                            x.Id,
                            x.OpeningPrice,
                            x.Start,
                            x.End,
                            x.Option.BarrierCount,
                            x.Option.BarrierStep,
                        }, cancellationToken);

                        foreach (var tradingWindow in tradingWindows)
                        {
                            var fullTime = (int)tradingWindow.End.Subtract(tradingWindow.Start).TotalSeconds;
                            var remaining = (int)tradingWindow.End.Subtract(now).TotalSeconds;
                            var delta = tradingWindow.BarrierCount * tradingWindow.BarrierStep;

                            int lower = tradingWindow.BarrierCount / 2;

                            var model = Enumerable.Range(0, tradingWindow.BarrierCount).Select(x =>
                            {
                                var barrier = tradingWindow.OpeningPrice + (x - lower) * tradingWindow.BarrierStep;

                                decimal high = 0;
                                decimal low = 0;

                                try
                                {
                                    high = 0.5m + ((currencyRate - barrier) / (1.25m * delta * remaining / fullTime));
                                    low = 0.5m + ((barrier - currencyRate) / (1.25m * delta * remaining / fullTime));
                                }
                                catch (DivideByZeroException)
                                {

                                }

                                return new BarrierViewModel
                                {
                                    Barrier = barrier,
                                    High = high,
                                    Low = low,
                                };
                            }).Reverse().ToArray();

                            await this.hubContext.Clients.Group(tradingWindow.Id).SendAsync("UpdateTradingWindow", model, cancellationToken);
                        }
                    }
                }

                await Task.Delay(GlobalConstants.CurrencyValueUpdateFrequency, cancellationToken);
            }
        }
    }
}
