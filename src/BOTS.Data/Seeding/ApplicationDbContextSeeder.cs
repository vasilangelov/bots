namespace BOTS.Data.Seeding
{
    public static class ApplicationDbContextSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            ISeeder[] seeders = new ISeeder[]
            {
                new NationalitySeeder(),
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(dbContext);
            }
        }
    }
}
