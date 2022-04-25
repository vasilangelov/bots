namespace BOTS.Services.Data.TradingWindows
{
    using System.Linq.Expressions;

    public interface ITradingWindowOptionService
    {
        Task<IEnumerable<T>> GetAllTradingWindowOptionsAsync<T>(Expression<Func<TradingWindowOption, T>> selector, CancellationToken cancellationToken = default);
    }
}
