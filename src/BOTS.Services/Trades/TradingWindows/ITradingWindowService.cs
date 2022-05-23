namespace BOTS.Services.Trades.TradingWindows
{
    public interface ITradingWindowService
    {
        Task<bool> IsTradingWindowActiveAsync(Guid tradingWindowId);

        Task<T> GetTradingWindowAsync<T>(Guid tradingWindowId);
    }
}
