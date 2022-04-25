namespace BOTS.Services.Data.TradingWindows
{
    using System.Linq.Expressions;

    public interface ITradingWindowService
    {
        Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(int currencyPairId, Expression<Func<TradingWindow, T>> selector, CancellationToken cancellationToken = default);

        Task<T?> GetTradingWindowAsync<T>(string tradingWindowId, Expression<Func<TradingWindow, T>> selector, CancellationToken cancellationToken = default);

        Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds, CancellationToken cancellationToken = default);
    }
}
