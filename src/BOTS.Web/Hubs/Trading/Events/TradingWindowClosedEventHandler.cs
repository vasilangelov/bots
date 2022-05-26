namespace BOTS.Web.Hubs.Trading.Events
{
    using System.Threading.Tasks;

    using BOTS.Services.Infrastructure.Events;
    using BOTS.Services.Trades.TradingWindows.Events;

    using Microsoft.AspNetCore.SignalR;

    public class TradingWindowClosedEventHandler : IEventHandler<TradingWindowClosedEvent>
    {
        private readonly IHubContext<TradingHub> tradingHub;

        public TradingWindowClosedEventHandler(IHubContext<TradingHub> tradingHub)
        {
            this.tradingHub = tradingHub;
        }

        public async Task InvokeAsync(TradingWindowClosedEvent context)
        {
            await this.tradingHub.Clients.All.SendAsync("RemoveEndedBet", context.TradingWindowId);
        }
    }
}
