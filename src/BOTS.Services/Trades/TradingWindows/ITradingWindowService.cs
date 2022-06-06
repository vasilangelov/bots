namespace BOTS.Services.Trades.TradingWindows
{
    public interface ITradingWindowService
    {
        Task UpdateEndedTradingWindowsAsync();

        Task EnsureAllTradingWindowsActiveAsync();
    }
}
