namespace BOTS.Services.Trades.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Data.Repositories;
    using BOTS.Services.Common;

    [TransientService]
    public class TradingWindowService : ITradingWindowService
    {
        private readonly IRepository<TradingWindow> tradingWindowRepository;
        private readonly IMapper mapper;

        public TradingWindowService(IRepository<TradingWindow> tradingWindowRepository, IMapper mapper)
        {
            this.tradingWindowRepository = tradingWindowRepository;
            this.mapper = mapper;
        }

        public async Task<T> GetTradingWindowAsync<T>(Guid tradingWindowId)
            => await this.tradingWindowRepository
                            .AllAsNotracking()
                            .Where(x => x.Id == tradingWindowId)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .FirstAsync();

        public async Task<bool> IsTradingWindowActiveAsync(Guid tradingWindowId)
            => await this.tradingWindowRepository
                            .AllAsNotracking()
                            .AnyAsync(x => x.Id == tradingWindowId && x.End > DateTime.UtcNow);
    }
}
