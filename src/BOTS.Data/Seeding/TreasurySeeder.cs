namespace BOTS.Data.Seeding
{
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;

    internal class TreasurySeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Treasuries.AnyAsync())
            {
                return;
            }

            Treasury treasury = new()
            {
                SystemBalance = 10_000,
                UserProfits = 0,
            };

            await dbContext.AddAsync(treasury);
            await dbContext.SaveChangesAsync();
        }
    }
}
