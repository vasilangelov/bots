namespace BOTS.Services.Data.CurrencyPairs
{
    using System.Linq.Expressions;

    using BOTS.Data.Models;

    public interface ICurrencyPairService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetActiveCurrenciesAsync();

        Task<IDictionary<string, IEnumerable<string>>> GetActiveCurrencyPairNamesAsync(CancellationToken cancellationToken = default);

        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId);

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(Expression<Func<CurrencyPair, T>> selector);

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default);
    }
}
