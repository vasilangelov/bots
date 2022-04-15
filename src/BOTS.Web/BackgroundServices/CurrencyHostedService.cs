namespace BOTS.Web.BackgroundServices
{
    using Microsoft.AspNetCore.SignalR;

    using BOTS.Services;
    using BOTS.Web.Hubs;
    using BOTS.Data.Models;
    using BOTS.Common;

    public class CurrencyHostedService : BackgroundService
    {
        private readonly ICurrencyProviderService currencyProviderService;
        private readonly IHubContext<CurrencyHub> currencyHub;
        private readonly IServiceProvider serviceProvider;

        private IEnumerable<KeyValuePair<string, string>> groupsAvailable = default!;

        public CurrencyHostedService(ICurrencyProviderService currencyProviderService,
                                     IHubContext<CurrencyHub> currencyHub,
                                     IServiceProvider serviceProvider)
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
                                        .Select(x => new KeyValuePair<string, string>(x.Left.Name, x.Right.Name))
                                        .ToArray();
            }

            if (this.groupsAvailable is null)
            {
                throw new NullReferenceException("Available groups not found");
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
                    decimal currencyRate = this.currencyProviderService
                                                    .GetCurrencyRate(group.Key, group.Value);

                    string groupName = string.Format(GlobalConstants.CurrencyPairFormat,
                                                     group.Key,
                                                     group.Value);

                    await this.currencyHub
                        .Clients
                        .Group(groupName)
                        .SendAsync("CurrencyRateUpdate", currencyRate, stoppingToken);
                }

                await Task.Delay(GlobalConstants.CurrencyValueUpdateFrequency, stoppingToken);
            }
        }
    }
}
