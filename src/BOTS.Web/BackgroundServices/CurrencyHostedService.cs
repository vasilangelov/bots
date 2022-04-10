namespace BOTS.Web.BackgroundServices
{
    using BOTS.Services;

    public class CurrencyHostedService : BackgroundService
    {
        private readonly ICurrencyProviderService currencyProviderService;

        public CurrencyHostedService(ICurrencyProviderService currencyProviderService)
        {
            this.currencyProviderService = currencyProviderService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await this.currencyProviderService.UpdateCurrencyInfoAsync(stoppingToken);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
