namespace BOTS.Services.Currencies.CurrencyRates
{
    public interface ICurrencyRateGeneratorService
    {
        void UpdateCurrencyRates();

        Task SeedInitialCurrencyRatesAsync();
    }
}
