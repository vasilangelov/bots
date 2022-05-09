namespace BOTS.Services.Data.TradingWindows
{
    public interface ITradingWindowOptionService
    {
        Task<IEnumerable<T>> GetAllTradingWindowOptionsAsync<T>();
    }
}
