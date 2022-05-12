namespace BOTS.Data.Seeding
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            var seeders = new ISeeder[]
            {
                new ApplicationSettingSeeder(),
                new NationalitySeeder(),
                new CurrencySeeder(),
                new CurrencyPairSeeder(),
                new TradingWindowOptionSeeder(),
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
