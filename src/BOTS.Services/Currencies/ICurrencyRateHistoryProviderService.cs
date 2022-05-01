namespace BOTS.Services.Currencies
{
    public interface ICurrencyRateHistoryProviderService
    {
        Task<IEnumerable<T>> GetCurrencyRateHistoryAsync<T>(
            string fromCurrency,
            string toCurrency,
            DateTime end,
            TimeSpan interval,
            TimeSpan timeRange,
            CancellationToken cancellationToken = default);

        Task<T> GetCurrencyRateHistoryAsync<T>(
            string fromCurrency,
            string toCurrency,
            CancellationToken cancellationToken = default);
    }
}
