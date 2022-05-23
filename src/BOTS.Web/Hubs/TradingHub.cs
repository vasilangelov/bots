namespace BOTS.Web.Hubs
{
    using System.Threading.Tasks;

    using BOTS.Common;
    using BOTS.Data.Models;
    using BOTS.Services.Balance;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.CurrencyRateStats;
    using BOTS.Services.Trades.Bets;
    using BOTS.Web.Extensions;
    using BOTS.Web.Models.ViewModels;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class TradingHub : Hub
    {
        private readonly IServiceProvider serviceProvider;

        public TradingHub(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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

            var currencyProviderService = scope.ServiceProvider.GetRequiredService<ICurrencyRateProviderService>();

            decimal currencyRate = await currencyProviderService.GetCurrencyRateAsync(currencyPairId);

            await this.Clients.Caller.SendAsync("UpdateCurrencyRate", currencyRate);

            var currencyRateStatService = scope.ServiceProvider.GetRequiredService<ICurrencyRateStatService>();

            var currencyRateStats = await currencyRateStatService.GetStatsAsync<CurrencyRateHistoryViewModel>(
                    currencyPairId,
                    // TODO: remove hardcoded temp values...
                    DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(10)),
                    TimeSpan.FromMilliseconds(GlobalConstants.CurrencyRateStatUpdateFrequency));

            await this.Clients.Caller.SendAsync("SetCurrencyRateHistory", currencyRateStats);
        }

        public async Task RemoveCurrencyRateSubscription(int currencyPairId)
        {
            await this.Groups
                .RemoveFromGroupAsync(this.Context.ConnectionId, currencyPairId.ToString());
        }

        public async Task AddBettingOptionSubscription(Guid bettingOptionId)
        {
            using var scope = this.serviceProvider.CreateScope();

            var tradingWindowService =
                scope.ServiceProvider.GetRequiredService<IBettingOptionService>();

            bool isBettingOptionActive =
                await tradingWindowService.IsBettingOptionActiveAsync(bettingOptionId);

            if (!isBettingOptionActive)
            {
                // TODO: display error message...
                return;
            }

            var end = await tradingWindowService.GetBettingOptionEndAsync(bettingOptionId);

            var model = DateTime.SpecifyKind(end, DateTimeKind.Utc).ToString("O");

            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, bettingOptionId.ToString());

            await this.Clients.Caller.SendAsync("UpdateTimer", model);
        }

        public async Task RemoveBettingOptionSubscription(Guid bettingOptionId)
        {
            await this.Groups
                .RemoveFromGroupAsync(this.Context.ConnectionId, bettingOptionId.ToString());
        }

        public async Task PlaceBettingOptionBet(
            // TODO: input model...
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout)
        {
            var userId = this.Context.User?.GetUserId();

            if (!userId.HasValue)
            {
                // TODO: display error message...
                return;
            }

            using var scope = this.serviceProvider.CreateScope();

            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var betViewModel =
                await betService.PlaceBetAsync<BetViewModel>(
                    userId.Value,
                    bettingOptionId,
                    betType,
                    barrier,
                    payout);

            await this.Clients.Caller.SendAsync("DisplayBet", betViewModel);

            var userService = scope.ServiceProvider.GetRequiredService<IBalanceService>();

            decimal balance = await userService.GetBalanceAsync(userId.Value);

            await this.Clients.Caller.SendAsync("UpdateBalance", balance);
        }

        public async Task GetActiveBets()
        {
            var userId = this.Context.User?.GetUserId();

            if (!userId.HasValue)
            {
                // TODO: display error message
                return;
            }

            using var scope = this.serviceProvider.CreateScope();

            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var model = await betService.GetActiveBetsAsync<BetViewModel>(userId.Value);

            await this.Clients.Caller.SendAsync("SetActiveBets", model);
        }

        public async Task GetBettingOptionsForCurrencyPair(int currencyPairId)
        {
            using var scope = this.serviceProvider.CreateScope();

            var bettingOptionService =
                scope.ServiceProvider.GetRequiredService<IBettingOptionService>();

            var model = await bettingOptionService
                .GetActiveBettingOptionsForCurrencyPairAsync<BettingOptionViewModel>(currencyPairId);

            await this.Clients.Caller.SendAsync("SetBettingOptions", model);
        }
    }
}
