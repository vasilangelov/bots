namespace BOTS.Services
{
    using Models;

    public interface ICurrencyProviderService
    {
        CurrencyInfo? GetCurrencyInfo();

        Task UpdateCurrencyInfoAsync(CancellationToken cancellationToken);
    }
}
