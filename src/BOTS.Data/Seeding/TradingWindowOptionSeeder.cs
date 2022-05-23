namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using BOTS.Data.Models;

    internal class TradingWindowOptionSeeder : ISeeder
    {
        private readonly IEnumerable<TradingWindowPreset> tradingWindowPresets
            = new TradingWindowPreset[] {
                new TradingWindowPreset
                {
                    Duration = TimeSpan.FromMinutes(10),
                },
                new TradingWindowPreset
                {
                    Duration = TimeSpan.FromMinutes(30),
                },
                new TradingWindowPreset
                {
                    Duration = TimeSpan.FromMinutes(60),
                },
            };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.TradingWindowPresets.AnyAsync())
            {
                return;
            }

            await dbContext.TradingWindowPresets.AddRangeAsync(this.tradingWindowPresets);
        }
    }
}
