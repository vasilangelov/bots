namespace BOTS.Services
{
    using BOTS.Services.Models;

    public interface ICurrencyProviderService
    {
        CurrencyInfo GetCurrencyInfo();

        Task UpdateCurrencyInfoAsync(CancellationToken cancellationToken = default);
    }
}
