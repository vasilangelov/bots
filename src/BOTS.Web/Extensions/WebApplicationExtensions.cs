namespace BOTS.Web.Extensions
{
    using BOTS.Data;
    using BOTS.Data.Seeding;
    using BOTS.Services.Currencies.CurrencyRates;

    using Microsoft.EntityFrameworkCore;

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

            await SeederManager.SeedAsync(dbContext);
        }

        public static async Task SeedCurrenciesAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyRateGeneratorService>();

            await currencyService.SeedInitialCurrencyRatesAsync();
        }
    }
}
