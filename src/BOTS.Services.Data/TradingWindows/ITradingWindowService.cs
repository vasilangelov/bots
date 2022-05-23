namespace BOTS.Services.Data.TradingWindows
{
    public interface ITradingWindowService
    {
        Task<IEnumerable<T>> GetActiveTradingWindowsByCurrencyPairAsync<T>(int currencyPairId);

        Task<T> GetTradingWindowAsync<T>(string tradingWindowId);

        Task UpdateEndedTradingWindowsAsync();

        Task EnsureAllTradingWindowsActiveAsync(IEnumerable<int> currencyPairIds);

        Task<bool> IsTradingWindowActiveAsync(string tradingWindowId);

        Task<int?> GetCurrencyPairIdAsync(string tradingWindowId);

        Task<decimal> GetEntryPercentageAsync(
            string tradingWindowId,
            BetType betType,
            byte barrierIndex);

        Task<decimal?> GetBarrierAsync(string tradingWindowId, byte barrierIndex);
    }
}
