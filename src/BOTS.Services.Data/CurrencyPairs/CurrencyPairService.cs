namespace BOTS.Services.Data.CurrencyPairs
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;

    public class CurrencyPairService : ICurrencyPairService
    {
        private readonly ICurrencyProviderService currencyProviderService;
        private readonly IRepository<CurrencyPair> currencyPairRepository;
        private readonly IMapper mapper;

        public CurrencyPairService(ICurrencyProviderService currencyProviderService, IRepository<CurrencyPair> currencyPairRepository, IMapper mapper)
        {
            this.currencyProviderService = currencyProviderService;
            this.currencyPairRepository = currencyPairRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetActiveCurrenciesAsync()
            => await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .Select(x => x.Left.Name)
                                .Concat(currencyPairRepository
                                    .AllAsNotracking()
                                    .Where(x => x.Display)
                                    .Select(x => x.Right.Name))
                                .Distinct()
                                .ToArrayAsync();

        public async Task<IDictionary<string, IEnumerable<string>>> GetActiveCurrencyPairNamesAsync(CancellationToken cancellationToken = default)
            => (await this.currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .Select(x => new { Left = x.Left.Name, Right = x.Right.Name })
                                .ToArrayAsync(cancellationToken))
                                .GroupBy(x => x.Left)
                                .ToDictionary(x => x.Key, x => x.Select(y => y.Right).AsEnumerable());

        public async Task<IEnumerable<int>> GetActiveCurrencyPairIdsAsync(CancellationToken cancellationToken = default)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .Select(x => x.Id)
                                .ToArrayAsync(cancellationToken);

        public async Task<IEnumerable<T>> GetActiveCurrencyPairsAsync<T>(CancellationToken cancellationToken = default)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Display)
                                .ProjectTo<T>(mapper.ConfigurationProvider)
                                .ToArrayAsync(cancellationToken);

        public async Task<decimal> GetCurrencyRateAsync(int currencyPairId,
                                                        CancellationToken cancellationToken = default)
        {
            (string left, string right) = await this.GetCurrencyPairNamesAsync(currencyPairId);

            return await this.currencyProviderService.GetCurrencyRateAsync(left, right, cancellationToken);
        }

        public async Task<bool> IsCurrencyPairActiveAsync(int currencyPairId)
            => await currencyPairRepository
                                .AllAsNotracking()
                                .AnyAsync(x => x.Id == currencyPairId && x.Display);

        private async Task<(string Left, string Right)> GetCurrencyPairNamesAsync(int currencyPairId)
        {
            var currencyPair = await currencyPairRepository
                                .AllAsNotracking()
                                .Where(x => x.Id == currencyPairId)
                                .Select(x => new { Left = x.Left.Name, Right = x.Right.Name })
                                .FirstAsync();

            return (currencyPair.Left, currencyPair.Right);
        }
    }
}
