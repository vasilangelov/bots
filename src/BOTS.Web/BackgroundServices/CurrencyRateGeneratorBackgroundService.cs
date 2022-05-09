﻿namespace BOTS.Web.BackgroundServices
{
    using BOTS.Common;
    using BOTS.Services.Currencies;

    public class CurrencyRateGeneratorBackgroundService : BackgroundService
    {
        private readonly ICurrencyRateGeneratorService currencyRateGeneratorService;

        public CurrencyRateGeneratorBackgroundService(ICurrencyRateGeneratorService currencyRateGeneratorService)
        {
            this.currencyRateGeneratorService = currencyRateGeneratorService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                this.currencyRateGeneratorService.UpdateCurrencyRates();

                await Task.Delay(GlobalConstants.CurrencyRateUpdateFrequency, cancellationToken);
            }
        }
    }
}
