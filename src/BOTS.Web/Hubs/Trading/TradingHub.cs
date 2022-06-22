namespace BOTS.Web.Hubs.Trading
{
    using System.Threading.Tasks;

    using BOTS.Common;
    using BOTS.Data.Models;
    using BOTS.Services.Common.Results;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.CurrencyRateStats;
    using BOTS.Services.Trades.Bets;
    using BOTS.Web.Extensions;
    using BOTS.Web.Models.ViewModels;
    using BOTS.Web.Resources;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Localization;

    using static BOTS.Services.Trades.Bets.BarrierActions;

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
                var stringLocalizer = (scope.ServiceProvider.GetRequiredService(typeof(IStringLocalizer<ValidationMessages>)) as IStringLocalizer<ValidationMessages>)!;

                await this.Clients.Caller.SendAsync("DisplayError", stringLocalizer["InvalidCurrencyPair"].Value);
                return;
            }

            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, currencyPairId.ToString());

            var currencyProviderService = scope.ServiceProvider.GetRequiredService<ICurrencyRateProviderService>();

            decimal currencyRate = await currencyProviderService.GetCurrencyRateAsync(currencyPairId);

            await this.Clients.Caller.SendAsync("UpdateCurrencyRate", currencyRate);

            var currencyRateStatService = scope.ServiceProvider.GetRequiredService<ICurrencyRateStatService>();

            var currencyRateStats = await currencyRateStatService.GetStatsAsync<CurrencyRateHistoryViewModel>(
                    currencyPairId,
                    DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(GlobalConstants.DisplayPastMinutesBetValues)),
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
                var stringLocalizer = (scope.ServiceProvider.GetRequiredService(typeof(IStringLocalizer<ValidationMessages>)) as IStringLocalizer<ValidationMessages>)!;

                await this.Clients.Caller.SendAsync("DisplayError", stringLocalizer["InvalidBettingOption"].Value);
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

        public async Task PlaceBet(
            Guid bettingOptionId,
            BetType betType,
            decimal barrier,
            decimal payout)
        {
            var userId = this.Context.User?.GetUserId();

            using var scope = this.serviceProvider.CreateScope();

            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var result = await betService.PlaceBetAsync(userId,
                                                       bettingOptionId,
                                                       betType,
                                                       barrier,
                                                       payout);

            if (result is ErrorResult<Guid> errorResult)
            {
                var stringLocalizer = scope
                    .ServiceProvider
                    .GetRequiredService<IStringLocalizer<ValidationMessages>>();

                await this.Clients
                    .Caller
                    .SendAsync(
                        "DisplayError",
                        stringLocalizer[errorResult.ErrorMessage, errorResult.Parameters].Value);
            }
            else if (result is SuccessResult<Guid> successResult)
            {
                var betViewModel = await betService.GetBetAsync<BetViewModel>(successResult.Value);

                await this.Clients.Caller.SendAsync("DisplayBet", betViewModel);
            }
        }

        public async Task GetActiveBets()
        {
            var userId = this.Context.User?.GetUserId();

            using var scope = this.serviceProvider.CreateScope();

            if (!userId.HasValue)
            {
                var stringLocalizer = (scope.ServiceProvider.GetRequiredService(typeof(IStringLocalizer<ValidationMessages>)) as IStringLocalizer<ValidationMessages>)!;

                await this.Clients.Caller.SendAsync("DisplayError", stringLocalizer["InvalidUser"].Value);
                return;
            }

            var betService = scope.ServiceProvider.GetRequiredService<IBetService>();

            var model = await betService.GetActiveUserBetsAsync<BetViewModel>(userId.Value);

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

        public async Task GetBarriers(Guid bettingOptionId)
        {
            var now = DateTime.UtcNow;

            using var scope = this.serviceProvider.CreateAsyncScope();

            var bettingOptionService = scope.ServiceProvider.GetRequiredService<IBettingOptionService>();

            bool isBettingOptionActive = await bettingOptionService.IsBettingOptionActiveAsync(bettingOptionId);

            if (!isBettingOptionActive)
            {
                var stringLocalizer = (scope.ServiceProvider.GetRequiredService(typeof(IStringLocalizer<ValidationMessages>)) as IStringLocalizer<ValidationMessages>)!;

                await this.Clients.Caller.SendAsync("DisplayError", stringLocalizer["InvalidBettingOption"].Value);
                return;
            }

            var bettingOption = await bettingOptionService.GetBettingOptionAsync<BettingOptionDto>(bettingOptionId);

            var currencyRateProviderService = scope.ServiceProvider.GetRequiredService<ICurrencyRateProviderService>();

            decimal currencyRate = await currencyRateProviderService
                .GetCurrencyRateAsync(bettingOption.CurrencyPairId);

            var remainingTime = (long)bettingOption.End.Subtract(now).TotalSeconds;

            var model = bettingOption.Barriers
                .Select(barrier => new BarrierViewModel
                {
                    Barrier = barrier,
                    High = GetEntryPercentage(bettingOption.Barriers,
                                              barrier,
                                              bettingOption.BarrierStep,
                                              currencyRate,
                                              remainingTime,
                                              bettingOption.Duration,
                                              BetType.Higher),
                    Low = GetEntryPercentage(bettingOption.Barriers,
                                             barrier,
                                             bettingOption.BarrierStep,
                                             currencyRate,
                                             remainingTime,
                                             bettingOption.Duration,
                                             BetType.Lower)
                })
                .ToArray();

            await this.Clients
                .Caller
                .SendAsync("UpdateBarriers", model);
        }
    }
}
