namespace BOTS.Services.Data.TradingWindows
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq.Expressions;
    using System.Threading;

    public class TradingWindowOptionService : ITradingWindowOptionService
    {
        private readonly IRepository<TradingWindowOption> tradingWindowOptionRepository;

        public TradingWindowOptionService(IRepository<TradingWindowOption> tradingWindowOptionRepository)
        {
            this.tradingWindowOptionRepository = tradingWindowOptionRepository;
        }

        // TODO: automapper...
        public async Task<IEnumerable<T>> GetAllTradingWindowOptionsAsync<T>(
            Expression<Func<TradingWindowOption, T>> selector,
            CancellationToken cancellationToken = default)
            => await this.tradingWindowOptionRepository
                            .AllAsNotracking()
                            .Select(selector)
                            .ToArrayAsync(cancellationToken);
    }
}
