namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Common;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Data.TradingWindows;
    using BOTS.Web.Hubs;
    using BOTS.Web.Models;

    public class TradingWindowBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<CurrencyHub> currencyHub;

        public TradingWindowBackgroundService(IServiceProvider serviceProvider, IHubContext<CurrencyHub> currencyHub)
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

                    IEnumerable<int> currencyPairIds = await currencyPairService.GetActiveCurrencyPairIdsAsync(cancellationToken);

                    var now = DateTime.UtcNow;

                    var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

                    await tradingWindowService.EnsureAllTradingWindowsActiveAsync(currencyPairIds, cancellationToken);

                    foreach (var currencyPairId in currencyPairIds)
                    {
                        decimal currencyRate = await currencyPairService.GetCurrencyRateAsync(currencyPairId, cancellationToken);

                        // TODO: reduce number of db requests...
                        var tradingWindows = await tradingWindowService.GetActiveTradingWindowsByCurrencyPairAsync<TradingWindowViewModel>(currencyPairId, cancellationToken);

                        foreach (var tradingWindow in tradingWindows)
                        {
                            var fullTime = (int)tradingWindow.End.Subtract(tradingWindow.Start).TotalSeconds;
                            var remaining = (int)tradingWindow.End.Subtract(now).TotalSeconds;
                            var delta = tradingWindow.OptionBarrierCount * tradingWindow.OptionBarrierStep;

                            int lower = tradingWindow.OptionBarrierCount / 2;

                            var model = Enumerable.Range(0, tradingWindow.OptionBarrierCount).Select(x =>
                            {
                                var barrier = tradingWindow.OpeningPrice + (x - lower) * tradingWindow.OptionBarrierStep;

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

                            await this.currencyHub.Clients.Group(tradingWindow.Id).SendAsync("UpdateTradingWindow", model, cancellationToken);
                        }
                    }
                }

                await Task.Delay(GlobalConstants.TradingWindowUpdateFrequency, cancellationToken);
            }
        }
    }
}
