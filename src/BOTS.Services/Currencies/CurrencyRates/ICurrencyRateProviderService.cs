namespace BOTS.Services.Currencies.CurrencyRates
{
    public interface ICurrencyRateProviderService
    {
        Task<decimal> GetCurrencyRateAsync(int currencyPairId);

        Task<decimal> GetPastCurrencyRateAsync(int currencyPairId, DateTime dateTime);
    }
}
