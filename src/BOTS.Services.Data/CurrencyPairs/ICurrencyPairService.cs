namespace BOTS.Services.Data.CurrencyPairs
{
    public interface ICurrencyPairService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetActiveCurrenciesAsync();

        Task<IDictionary<string, IEnumerable<string>>> GetActiveCurrencyPairNamesAsync(CancellationToken cancellationToken = default);

        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId);

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(CancellationToken cancellationToken = default);

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default);
    }
}
