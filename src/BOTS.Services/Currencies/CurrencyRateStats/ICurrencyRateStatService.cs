namespace BOTS.Services.CurrencyRateStats
{
    public interface ICurrencyRateStatService
    {
        Task<T> GetLatestStatAsync<T>(int currencyPairId);

        Task<IEnumerable<T>> GetStatsAsync<T>(
            int currencyPairId,
            DateTime start,
            TimeSpan interval);
    }
}
