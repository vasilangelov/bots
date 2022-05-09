namespace BOTS.Services.Data.CurrencyPairs
{
    public interface ICurrencyPairService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId);

        Task<decimal> GetPastCurrencyRateAsync(int currencyPairId, DateTime dateTime);

        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId);

        Task<(string, string)> GetCurrencyPairNamesAsync(int currencyPairId);

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>();

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync();

        Task<IEnumerable<(string, string)>> GetAllActiveCurrencyPairNamesAsync();
    }
}
