namespace BOTS.Services.Currencies
{
    public interface ICurrencyRateGeneratorService
    {
        void UpdateCurrencyRates();

        Task SeedInitialCurrencyRatesAsync(IEnumerable<(string, string)> currencyPairs, CancellationToken cancellationToken = default);
    }
}
