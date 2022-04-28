namespace BOTS.Services.Currencies
{
    public interface ICurrencyRateProviderService
    {
        Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            CancellationToken cancellationToken = default);
    }
}
