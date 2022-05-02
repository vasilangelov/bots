namespace BOTS.Web.Hubs
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Data.TradingWindows;
    using BOTS.Web.Models;
    using BOTS.Services.Currencies;
    using BOTS.Common;

    [Authorize]
    public class CurrencyHub : Hub
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICurrencyRateStatProviderService currencyRateHistoryProviderService;

        public CurrencyHub(IServiceProvider serviceProvider,
                           ICurrencyRateStatProviderService currencyRateHistoryProviderService)
        {
            this.serviceProvider = serviceProvider;
            this.currencyRateHistoryProviderService = currencyRateHistoryProviderService;
        }

        public async Task AddCurrencyRateSubscription(int currencyPairId)
        {
            using var scope = this.serviceProvider.CreateScope();

            var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

            var isCurrencyPairActive = await currencyPairService.IsCurrencyPairActiveAsync(currencyPairId);

            if (!isCurrencyPairActive)
            {
                // TODO: display error message...
                return;
            }

            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, currencyPairId.ToString());

            decimal currencyRate = await currencyPairService.GetCurrencyRateAsync(currencyPairId);

            await this.Clients.Caller.SendAsync("UpdateCurrencyRate", currencyRate);

            var (fromCurrency, toCurrency) = await currencyPairService.GetCurrencyPairNamesAsync(currencyPairId);

            var currencyRateStats = await this.currencyRateHistoryProviderService
                .GetLatestCurrencyRateStatsAsync<CurrencyRateHistoryViewModel>(
                    fromCurrency,
                    toCurrency,
                    // TODO: remove hardcoded temp values...
                    DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)),
                    TimeSpan.FromMilliseconds(GlobalConstants.CurrencyRateStatUpdateFrequency));

            await this.Clients.Caller.SendAsync("SetCurrencyRateHistory", currencyRateStats);
        }

        public async Task RemoveCurrencyRateSubscription(int currencyPairId)
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, currencyPairId.ToString());
        }

        public async Task AddTradingWindowSubscription(string tradingWindowId)
        {
            using var scope = this.serviceProvider.CreateScope();
            var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

            bool isTradingWindowActive = await tradingWindowService.IsTradingWindowActiveAsync(tradingWindowId);

            if (!isTradingWindowActive)
            {
                // TODO: display error message...
                return;
            }

            var model = await tradingWindowService.GetTradingWindowAsync<TradingWindowEndViewModel>(tradingWindowId);

            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, tradingWindowId);

            await this.Clients.Caller.SendAsync("UpdateTimer", model);
        }

        public async Task RemoveTradingWindowSubscription(string tradingWindowId)
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, tradingWindowId);
        }

        public async Task GetActiveTradingWindows(int currencyPairId)
        {
            using var scope = this.serviceProvider.CreateScope();

            var tradingWindowService = scope.ServiceProvider.GetRequiredService<ITradingWindowService>();

            var result = await tradingWindowService.GetActiveTradingWindowsByCurrencyPairAsync<ActiveTradingWindowViewModel>(currencyPairId);

            await this.Clients.Caller.SendAsync("SetTradingWindows", result);
        }
    }
}
