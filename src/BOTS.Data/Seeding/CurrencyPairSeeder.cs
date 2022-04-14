namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using Models;

    internal class CurrencyPairSeeder : ISeeder
    {
        private readonly Dictionary<string, string[]> currencyPairNames = new()
        {
            {
                "AUD",
                new string[]
                {
                    "JPY",
                    "USD",
                }
            },
            {
                "EUR",
                new string[]
                {
                    "GBP",
                    "JPY",
                    "USD",
                }
            },
            {
                "GBP",
                new string[]
                {
                    "JPY",
                    "USD",
                }
            },
            {
                "USD",
                new string[]
                {
                    "CAD",
                    "JPY",
                }
            },
        };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.CurrencyPairs.AnyAsync())
            {
                return;
            }

            var currencies = await dbContext.Currencies.ToDictionaryAsync(c => c.Name, c => c.Id);

            var currencyPairs = currencyPairNames
                .SelectMany(kvp => kvp.Value.Select(to => new CurrencyPair
                {
                    LeftId = currencies[kvp.Key],
                    RightId = currencies[to],
                    Display = true,
                }))
                .ToArray();

            await dbContext.AddRangeAsync(currencyPairs);
        }
    }
}
