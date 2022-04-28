namespace BOTS.Web.Extensions
{
    using Microsoft.EntityFrameworkCore;

    using BOTS.Data;
    using BOTS.Data.Seeding;
    using BOTS.Services.Data.CurrencyPairs;
    using BOTS.Services.Currencies;

    public static class WebApplicationExtensions
    {
        public static async Task MigrateDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await dbContext.Database.MigrateAsync();
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await ApplicationDbContextSeeder.SeedAsync(dbContext);
        }

        public static async Task SeedCurrenciesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var currencyPairService = scope.ServiceProvider.GetRequiredService<ICurrencyPairService>();

            var currencyRateGeneratorService = scope.ServiceProvider.GetRequiredService<ICurrencyRateGeneratorService>();

            var activeCurrencies = await currencyPairService.GetActiveCurrencyPairNamesAsync();

            await currencyRateGeneratorService.SeedInitialCurrencyRatesAsync(activeCurrencies);
        }
    }
}
