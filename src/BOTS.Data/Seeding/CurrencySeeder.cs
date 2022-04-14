namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using Models;

    internal class CurrencySeeder : ISeeder
    {
        private static readonly string[] currencyNames =
        {
            "AUD",
            "CAD",
            "USD",
            "EUR",
            "GBP",
            "JPY",
        };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Currencies.AnyAsync())
            {
                return;
            }

            IEnumerable<Currency> currencies = currencyNames.Select(name => new Currency
            {
                Name = name,
            });

            await dbContext.AddRangeAsync(currencies);
        }
    }
}
