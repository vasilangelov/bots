namespace BOTS.Services
{
    public interface ICurrencyProviderService
    {
        decimal GetCurrencyRate(string left, string right);

        Task UpdateCurrencyInfoAsync(CancellationToken cancellationToken = default);
    }
}
