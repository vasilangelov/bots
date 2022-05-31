namespace BOTS.Services.Currencies.CurrencyRates
{
    using System.Collections.Concurrent;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Services.Common;
    using BOTS.Services.Infrastructure.Extensions;

    using Microsoft.Extensions.Caching.Memory;

    [TransientService]
    public class CurrencyPairService : ICurrencyPairService
    {
        private static readonly object currencyRateNamesKey = new();

        private readonly IRepository<CurrencyPair> currencyPairRepository;
        private readonly IMemoryCache memoryCache;
        private readonly IMapper mapper;

        public CurrencyPairService(
            IRepository<CurrencyPair> currencyPairRepository,
            IMemoryCache memoryCache,
            IMapper mapper)
        {
            this.currencyPairRepository = currencyPairRepository;
            this.memoryCache = memoryCache;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync()
            => await this.currencyPairRepository
                            .AllAsNoTracking()
                            .Where(x => x.Display)
                            .Select(x => x.Id)
                            .ToArrayAsync();

        public async Task<IEnumerable<(string, string)>> GetAllActiveCurrencyPairNamesAsync()
            => await this.currencyPairRepository
                                .AllAsNoTracking()
                                .Where(x => x.Display)
                                .Select(x => new Tuple<string, string>(x.CurrencyFrom.Name, x.CurrencyTo.Name).ToValueTuple())
                                .ToArrayAsync();

        public async Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>()
            => await currencyPairRepository
                                .AllAsNoTracking()
                                .Where(x => x.Display)
                                .ProjectTo<T>(mapper.ConfigurationProvider)
                                .ToArrayAsync();


        public async Task<bool> IsCurrencyPairActiveAsync(int currencyPairId)
            => await currencyPairRepository
                                .AllAsNoTracking()
                                .AnyAsync(x => x.Id == currencyPairId && x.Display);

        public async Task<(string, string)> GetCurrencyPairNamesAsync(int currencyPairId)
        {
            var currencyRateNames = this.memoryCache.GetOrAdd<ConcurrentDictionary<int, (string, string)>>(currencyRateNamesKey, () => new());

            if (!currencyRateNames.ContainsKey(currencyPairId))
            {
                var (from, to) = await this.currencyPairRepository
                                .AllAsNoTracking()
                                .Where(x => x.Id == currencyPairId)
                                .Select(x =>
                                    new Tuple<string, string>(x.CurrencyFrom.Name, x.CurrencyTo.Name)
                                        .ToValueTuple())
                                .FirstAsync();

                currencyRateNames.TryAdd(currencyPairId, (from, to));
            }

            return currencyRateNames[currencyPairId];
        }
    }
}
