namespace BOTS.Services
{
    public interface ICurrencyProviderService
    {
        Task<decimal> GetCurrencyRateAsync(string baseCurrency,
                                        string convertCurrency,
                                        CancellationToken cancellationToken = default);

        Task UpdateCurrencyRatesAsync(IDictionary<string, IEnumerable<string>> currencyPairs,
                                                   CancellationToken cancellationToken = default);
    }
}
