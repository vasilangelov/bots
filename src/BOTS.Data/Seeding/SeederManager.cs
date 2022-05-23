namespace BOTS.Data.Seeding
{
    public static class SeederManager
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
                new BettingOptionPresetSeeder(),
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
