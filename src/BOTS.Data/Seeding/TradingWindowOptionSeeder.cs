namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using BOTS.Data.Models;

    internal class TradingWindowOptionSeeder : ISeeder
    {
        private readonly IEnumerable<TradingWindowOption> tradingWindowOptions
            = new TradingWindowOption[] {
                new TradingWindowOption
                {
                    Duration = TimeSpan.FromMinutes(10),
                    BarrierStep = 0.05m,
                    BarrierCount = 5,
                },
                new TradingWindowOption
                {
                    Duration = TimeSpan.FromMinutes(30),
                    BarrierStep = 0.05m,
                    BarrierCount = 5,
                },
                new TradingWindowOption
                {
                    Duration = TimeSpan.FromMinutes(60),
                    BarrierStep = 0.05m,
                    BarrierCount = 5,
                },
            };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.TradingWindowOptions.AnyAsync())
            {
                return;
            }

            await dbContext.TradingWindowOptions.AddRangeAsync(this.tradingWindowOptions);
        }
    }
}
