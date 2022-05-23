namespace BOTS.Services.Currencies.CurrencyRates
{
    public interface IThirdPartyCurrencyRateProviderService
    {
        Task<IDictionary<(string FromCurrency, string ToCurrency), decimal>> GetCurrencyRatesAsync(
            IEnumerable<(string FromCurrency, string ToCurrency)> currencyPairs);

        Task<decimal> GetCurrencyRateAsync(string fromCurrency, string toCurrency);

        Task<decimal> GetPastCurrencyRateAsync(
            string fromCurrency,
            string toCurrency,
            DateTime dateTime);
    }
}
