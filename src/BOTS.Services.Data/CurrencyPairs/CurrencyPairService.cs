namespace BOTS.Services.Data.CurrencyPairs
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using System.Collections.Concurrent;

    using BOTS.Services.Currencies;

    public class CurrencyPairService : ICurrencyPairService
    {
        private static readonly object currencyRateNamesKey = new();

        private readonly ICurrencyRateProviderService currencyRateProviderService;
        private readonly IMemoryCache memoryCache;
        private readonly IRepository<CurrencyPair> currencyPairRepository;
        private readonly IMapper mapper;

        public CurrencyPairService(
            ICurrencyRateProviderService currencyRateProviderService,
            IMemoryCache memoryCache,
            IRepository<CurrencyPair> currencyPairRepository,
            IMapper mapper)
        {
            this.currencyRateProviderService = currencyRateProviderService;
            this.memoryCache = memoryCache;
            this.currencyPairRepository = currencyPairRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .Select(x => x.Id)
                                .ToArrayAsync(cancellationToken);

        public async Task<IEnumerable<(string, string)>> GetActiveCurrencyPairNamesAsync(
            CancellationToken cancellationToken = default)
            => await this.currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .Select(x => new Tuple<string, string>(x.Left.Name, x.Right.Name).ToValueTuple())
                                .ToArrayAsync(cancellationToken);

        public async Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(CancellationToken cancellationToken = default)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .ProjectTo<T>(mapper.ConfigurationProvider)
                                .ToArrayAsync(cancellationToken);

        public async Task<decimal> GetCurrencyRateAsync(
            int currencyPairId,
            CancellationToken cancellationToken = default)
        {
            ConcurrentDictionary<int, (string, string)> currencyRateNames;

            lock (currencyRateNamesKey)
            {
                currencyRateNames = this.memoryCache.GetOrCreate<ConcurrentDictionary<int, (string, string)>>(currencyRateNamesKey, _ => new());
            }

            if (!currencyRateNames.ContainsKey(currencyPairId))
            {
                var (from, to) = await this.GetCurrencyPairNamesAsync(currencyPairId, cancellationToken);

                currencyRateNames.TryAdd(currencyPairId, (from, to));
            }

            var (fromCurrency, toCurrency) = currencyRateNames[currencyPairId];

            return await this.currencyRateProviderService.GetCurrencyRateAsync(fromCurrency, toCurrency, cancellationToken);
        }

        public async Task<bool> IsCurrencyPairActiveAsync(int currencyPairId, CancellationToken cancellationToken = default)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .AnyAsync(x => x.Id == currencyPairId && x.Display, cancellationToken);

        private async Task<(string FromCurrency, string ToCurrency)> GetCurrencyPairNamesAsync(int currencyPairId, CancellationToken cancellationToken = default)
        {
            var currencyPair = await this.currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Id == currencyPairId)
                                .Select(x => new { Left = x.Left.Name, Right = x.Right.Name })
                                .FirstAsync(cancellationToken);

            return (currencyPair.Left, currencyPair.Right);
        }
    }
}
