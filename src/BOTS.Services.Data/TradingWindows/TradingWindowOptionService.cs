namespace BOTS.Services.Data.TradingWindows
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    public class TradingWindowOptionService : ITradingWindowOptionService
    {
        private readonly IRepository<TradingWindowOption> tradingWindowOptionRepository;
        private readonly IMapper mapper;

        public TradingWindowOptionService(
            IRepository<TradingWindowOption> tradingWindowOptionRepository,
            IMapper mapper)
        {
            this.tradingWindowOptionRepository = tradingWindowOptionRepository;
            this.mapper = mapper;
        }

        // TODO: options could be cached...
        public async Task<IEnumerable<T>> GetAllTradingWindowOptionsAsync<T>()
            => await this.tradingWindowOptionRepository
                            .AllAsNotracking()
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .ToArrayAsync();
    }
}
