namespace BOTS.Data.Seeding
{
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;

    internal class BettingOptionPresetSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.BettingOptionPresets.AnyAsync())
            {
                return;
            }

            var bettingOptions = await dbContext.CurrencyPairs
                            .SelectMany(pair => dbContext.TradingWindowPresets
                                .Select(windowPreset => new BettingOptionPreset
                                {
                                    BarrierCount = 5,
                                    BarrierStep = 0.005m,
                                    CurrencyPairId = pair.Id,
                                    TradingWindowPresetId = windowPreset.Id
                                }))
                            .ToArrayAsync();

            await dbContext.AddRangeAsync(bettingOptions);
        }
    }
}
