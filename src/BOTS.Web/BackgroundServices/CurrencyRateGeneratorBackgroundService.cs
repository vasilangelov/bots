namespace BOTS.Web.BackgroundServices
{
    using BOTS.Common;
    using BOTS.Services.Currencies.CurrencyRates;

    public class CurrencyRateGeneratorBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public CurrencyRateGeneratorBackgroundService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var currencyRateGenerator =
                        scope.ServiceProvider.GetRequiredService<ICurrencyRateGeneratorService>();

                    currencyRateGenerator.UpdateCurrencyRates();
                }

                await Task.Delay(GlobalConstants.CurrencyRateUpdateFrequency, cancellationToken);
            }
        }
    }
}
