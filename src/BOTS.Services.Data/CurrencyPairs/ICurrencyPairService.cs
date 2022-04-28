namespace BOTS.Services.Data.CurrencyPairs
{
    public interface ICurrencyPairService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<IEnumerable<(string, string)>> GetActiveCurrencyPairNamesAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(CancellationToken cancellationToken = default);

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default);
    }
}
