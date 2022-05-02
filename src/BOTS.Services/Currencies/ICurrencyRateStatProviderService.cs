namespace BOTS.Services.Currencies
{
    public interface ICurrencyRateStatProviderService
    {
        Task<T> GetLatestCurrencyRateStatAsync<T>(
            string fromCurrency,
            string toCurrency,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetLatestCurrencyRateStatsAsync<T>(
            string fromCurrency,
            string toCurrency,
            DateTime start,
            TimeSpan interval,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetCurrencyRateStatsAsync<T>(
            string fromCurrency,
            string toCurrency,
            DateTime start,
            DateTime end,
            TimeSpan interval,
            CancellationToken cancellationToken = default);
    }
}
