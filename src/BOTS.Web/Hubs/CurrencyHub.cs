namespace BOTS.Web.Hubs
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    using BOTS.Data.Models;
    using BOTS.Services;
    using BOTS.Common;

    [Authorize]
    public class CurrencyHub : Hub
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ICurrencyProviderService currencyProvider;

        public CurrencyHub(IServiceProvider serviceProvider, ICurrencyProviderService currencyProvider)
        {
            this.serviceProvider = serviceProvider;
            this.currencyProvider = currencyProvider;
        }

        public async Task AddCurrencySubscription(string left, string right)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                // TODO: extract to service...
                var currencyPairRepository = scope.ServiceProvider.GetRequiredService<IRepository<CurrencyPair>>();

                var isCurrencySupported = await currencyPairRepository
                    .AllAsNotracking()
                    .AnyAsync(x => x.Display && x.Left.Name == left && x.Right.Name == right);

                if (!isCurrencySupported)
                {
                    // TODO: Display user error message...
                    return;
                }
            }

            string groupName = string.Format(GlobalConstants.CurrencyPairFormat, left, right);
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

            decimal currencyRate = this.currencyProvider.GetCurrencyRate(left, right);
            await this.Clients.Client(this.Context.ConnectionId).SendAsync("CurrencyRateUpdate", currencyRate);
        }

        public async Task RemoveCurrencySubscription(string left, string right)
        {
            string groupName = string.Format(GlobalConstants.CurrencyPairFormat, left, right);
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);
        }
    }
}
