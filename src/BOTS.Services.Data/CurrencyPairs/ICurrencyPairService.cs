namespace BOTS.Services.Data.CurrencyPairs
{
    public interface ICurrencyPairService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<(string, string)> GetCurrencyPairNamesAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(CancellationToken cancellationToken = default);

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<(string, string)>> GetAllActiveCurrencyPairNamesAsync(CancellationToken cancellationToken = default);
    }
}
