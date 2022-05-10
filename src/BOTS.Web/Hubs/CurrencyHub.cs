namespace BOTS.Web.Hubs
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;
    using System.Security.Claims;

    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Data.TradingWindows;
    using BOTS.Services.Currencies;
    using BOTS.Common;
    using BOTS.Services.Data.Bets;
    using BOTS.Data.Models;
    using BOTS.Web.Models.ViewModels;
    using BOTS.Services.Data.Users;
    using BOTS.Web.Extensions;

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

        public async Task PlaceTradingWindowBet(
            // TODO: input model...
            string tradingWindowId,
            BetType betType,
            byte barrierNumber,
            decimal payout)
        {
            var userId = this.Context.User?.GetUserId();

            if (userId is null)
            {
                // TODO: display error message...
                return;
            }

            using var scope = this.serviceProvider.CreateScope();


            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var betViewModel = await betService.PlaceBetAsync<BetViewModel>(userId, betType, tradingWindowId, barrierNumber, payout);

            await this.Clients.Caller.SendAsync("DisplayBet", betViewModel);

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            decimal balance = await userService.GetUserBalance(userId);

            await this.Clients.Caller.SendAsync("UpdateBalance", balance);
        }

        public async Task GetActiveBets()
        {
            var userId = this.Context.User?.GetUserId();

            if (userId is null)
            {
                // TODO: display error message
                return;
            }

            using var scope = this.serviceProvider.CreateScope();

            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var model = await betService.GetActiveBetsAsync<BetViewModel>(userId);

            await this.Clients.Caller.SendAsync("SetActiveBets", model);
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
