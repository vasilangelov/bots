namespace BOTS.Web.BackgroundServices
{
    using BOTS.Common;
    using BOTS.Data.Models;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.Trades.Bets;
    using BOTS.Services.Trades.TradingWindows;
    using BOTS.Web.Hubs.Trading;
    using BOTS.Web.Models.ViewModels;

    using Microsoft.AspNetCore.SignalR;

    using static BOTS.Services.Trades.Bets.BarrierActions;

    public class BettingOptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IHubContext<TradingHub> currencyHub;

        public BettingOptionBackgroundService(
            IServiceProvider serviceProvider,
            IHubContext<TradingHub> currencyHub)
        {
            this.serviceProvider = serviceProvider;
            this.currencyHub = currencyHub;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

                await tradingWindowService.UpdateEndedTradingWindowsAsync();
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var now = DateTime.UtcNow;

                    var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

                    await tradingWindowService.EnsureAllTradingWindowsActiveAsync();

                    var bettingOptionService =
                        scope.ServiceProvider.GetRequiredService<IBettingOptionService>();

                    var bettingOptions = await bettingOptionService
                        .GetAllActiveBettingOptionsAsync<BettingOptionDto>();

                    await Parallel.ForEachAsync(
                            bettingOptions,
                            async (bo, ct) =>
                            {
                                using var newScope = this.serviceProvider.CreateAsyncScope();

                                var currencyRateProviderService = newScope.ServiceProvider.GetRequiredService<ICurrencyRateProviderService>();

                                decimal currencyRate = await currencyRateProviderService
                                    .GetCurrencyRateAsync(bo.CurrencyPairId);

                                var remainingTime = (long)bo.End.Subtract(now).TotalSeconds;

                                var model = bo.Barriers
                                    .Select(barrier => new BarrierViewModel
                                    {
                                        Barrier = barrier,
                                        High = GetEntryPercentage(bo.Barriers,
                                                                  barrier,
                                                                  bo.BarrierStep,
                                                                  currencyRate,
                                                                  remainingTime,
                                                                  bo.Duration,
                                                                  BetType.Higher),
                                        Low = GetEntryPercentage(bo.Barriers,
                                                                 barrier,
                                                                 bo.BarrierStep,
                                                                 currencyRate,
                                                                 remainingTime,
                                                                 bo.Duration,
                                                                 BetType.Lower)
                                    })
                                    .ToArray();

                                await this.currencyHub.Clients
                                    .Group(bo.Id.ToString())
                                    .SendAsync("UpdateBarriers", model, ct);
                            });
                }

                await Task.Delay(GlobalConstants.TradingWindowUpdateFrequency, cancellationToken);
            }
        }
    }
}
