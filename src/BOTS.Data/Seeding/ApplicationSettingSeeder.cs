namespace BOTS.Data.Seeding
{
    using Microsoft.EntityFrameworkCore;

    using BOTS.Data.Models;

    internal class ApplicationSettingSeeder : ISeeder
    {
        private static readonly Dictionary<string, string> settings = new()
        {
            { "InitialBalance", "10000" },
            { "DefaultUserPreset", @"{ ""CurrencyPairId"": 0, ""ChartType"": 0, ""Payout"": 10 }" },
        };

        public async Task SeedAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.ApplicationSettings.AnyAsync())
            {
                return;
            }

            var applicationSettings = settings.Select(x => new ApplicationSetting
            {
                Key = x.Key,
                Value = x.Value
            }).ToArray();

            await dbContext.AddRangeAsync(applicationSettings);
        }
    }
}
