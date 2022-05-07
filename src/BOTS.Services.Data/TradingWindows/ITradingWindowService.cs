namespace BOTS.Services.Data.TradingWindows
{
    public interface ITradingWindowService
    {
        decimal CalculateBarrier(byte barrierIndex, int barrierCount, decimal openingPrice, decimal barrierStep);

        Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(int currencyPairId, CancellationToken cancellationToken = default);

        Task<T> GetTradingWindowAsync<T>(string tradingWindowId, CancellationToken cancellationToken = default);

        Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds, CancellationToken cancellationToken = default);

        Task<bool> IsTradingWindowActiveAsync(string tradingWindowId, CancellationToken cancellationToken = default);

        Task<int?> GetCurrencyPairIdAsync(string tradingWindowId);

        Task<decimal> GetEntryPercentageAsync(
            string tradingWindowId,
            BetType betType,
            byte barrierIndex);
    }
}
