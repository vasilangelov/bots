namespace BOTS.Services.Trades.TradingWindows
{
    public interface ITradingWindowManagerService
    {
        Task UpdateEndedTradingWindowsAsync();

        Task EnsureAllTradingWindowsActiveAsync();
    }
}
