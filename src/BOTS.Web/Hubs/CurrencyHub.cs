namespace BOTS.Web.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;

    using BOTS.Data.Models;
    using BOTS.Services;
    using Microsoft.AspNetCore.Authorization;

    [Authorize]
    public class CurrencyHub : Hub
    {
        private readonly IServiceProvider serviceProvider;

        public CurrencyHub(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
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

            string groupName = $"{left}/{right}";
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);
        }

        public async Task RemoveCurrencySubscription(string left, string right)
        {
            string groupName = $"{left}/{right}";
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);
        }
    }
}
