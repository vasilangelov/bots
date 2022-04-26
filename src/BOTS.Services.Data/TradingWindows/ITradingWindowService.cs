namespace BOTS.Services.Data.TradingWindows
{
    public interface ITradingWindowService
    {
        Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(int currencyPairId, CancellationToken cancellationToken = default);

        Task<T> GetTradingWindowAsync<T>(string tradingWindowId, CancellationToken cancellationToken = default);

        Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds, CancellationToken cancellationToken = default);

        Task<bool> IsTradingWindowActiveAsync(string tradingWindowId, CancellationToken cancellationToken = default);
    }
}
