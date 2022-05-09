namespace BOTS.Services.Currencies
{
    public interface ICurrencyRateProviderService
    {
        Task<decimal> GetCurrencyRateAsync(
            string fromCurrency,
            string toCurrency);

        Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime);
    }
}
