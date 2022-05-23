namespace BOTS.Services.Currencies.CurrencyRates
{
    public interface ICurrencyPairService
    {
        Task<bool> IsCurrencyPairActiveAsync(int currencyPairId);

        Task<(string, string)> GetCurrencyPairNamesAsync(int currencyPairId);

        Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync();

        Task<IEnumerable<(string, string)>> GetAllActiveCurrencyPairNamesAsync();

        Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>();
    }
}
