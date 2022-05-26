namespace BOTS.Web.Hubs.Trading.Events
{
    using BOTS.Services.Balance.Events;
    using BOTS.Services.Infrastructure.Events;

    using Microsoft.AspNetCore.SignalR;

    public class UpdateBalanceEventHandler : IEventHandler<UpdateBalanceEvent>
    {
        private readonly IHubContext<TradingHub> tradingHub;

        public UpdateBalanceEventHandler(IHubContext<TradingHub> tradingHub)
        {
            this.tradingHub = tradingHub;
        }

        public async Task InvokeAsync(UpdateBalanceEvent context)
        {
            string userId = context.UserId.ToString().ToLower();

            await this.tradingHub
                .Clients
                .User(userId)
                .SendAsync("UpdateBalance", context.Balance);
        }
    }
}
