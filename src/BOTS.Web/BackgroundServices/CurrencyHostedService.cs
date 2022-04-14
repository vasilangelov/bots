namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Services;
    using BOTS.Web.Hubs;
    using BOTS.Data.Models;

    public class CurrencyHostedService : BackgroundService
    {
        private readonly ICurrencyProviderService currencyProviderService;
        private readonly IHubContext<CurrencyHub> currencyHub;
        private readonly IServiceProvider serviceProvider;

        private IEnumerable<string> groupsAvailable = default!;

        public CurrencyHostedService(ICurrencyProviderService currencyProviderService, IHubContext<CurrencyHub> currencyHub, IServiceProvider serviceProvider)
        {
            this.currencyProviderService = currencyProviderService;
            this.currencyHub = currencyHub;
            this.serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = this.serviceProvider.CreateScope())
            {
                var currencyPair = scope.ServiceProvider.GetRequiredService<IRepository<CurrencyPair>>();

                this.groupsAvailable = currencyPair
                                        .AllAsNotracking()
                                        .Where(x => x.Display)
                                        // TODO: add specific format...
                                        .Select(x => x.Left.Name + "/" + x.Right.Name)
                                        .ToArray();
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await this.currencyProviderService.UpdateCurrencyInfoAsync(stoppingToken);

                foreach (var group in groupsAvailable)
                {
                    string[] currencies = group.Split("/");

                    decimal currencyRate = this.currencyProviderService.GetCurrencyRate(currencies[0], currencies[1]);

                    // TODO: if group does not exist???
                    // TODO: Action constants...
                    await this.currencyHub.Clients.Group(group).SendAsync("CurrencyRateUpdate", currencyRate, stoppingToken);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
