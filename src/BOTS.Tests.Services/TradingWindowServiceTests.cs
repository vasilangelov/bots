namespace BOTS.Tests.Services
{
    using System;
    using System.Threading.Tasks;

    using BOTS.Data;
    using BOTS.Data.Models;
    using BOTS.Services.Trades.TradingWindows;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using Xunit;

    public class TradingWindowServiceTests : TestBase
    {
        [Fact]
        public async Task ShouldUpdateEndedTradingWindows()
        {
            await this.SeedTradingWindowsAsync();

            var db = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var tradingWindowCount = await db.TradingWindows.CountAsync(x => !x.IsClosed);
            var expectedToBeClosed = await db.TradingWindows.CountAsync(x => !x.IsClosed && DateTime.UtcNow >= x.End);

            var expected = tradingWindowCount - expectedToBeClosed;

            var tradingWindowService = this.serviceProvider.GetRequiredService<ITradingWindowService>();

            await tradingWindowService.UpdateEndedTradingWindowsAsync();

            var actual = await db.TradingWindows.CountAsync(x => !x.IsClosed);

            Assert.Equal(expected, actual);
        }

        [Fact]
        private async Task ShouldEnsureAllTradingWindowsAreCreatedAndActive()
        {
            await this.SeedTradingWindowsAsync();
            await this.SeedBettingOptionPresetsAsync();

            var db = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var expectedToBeRemoved = await db.TradingWindows.CountAsync(x => !x.IsClosed && DateTime.UtcNow >= x.End);
            var expectedToBeAdded = await db.BettingOptionPresets.CountAsync();
            var currentCount = await db.TradingWindows.CountAsync(x => !x.IsClosed);

            var expected = currentCount + expectedToBeAdded - expectedToBeRemoved;

            var tradingWindowService = this.serviceProvider.GetRequiredService<ITradingWindowService>();

            await tradingWindowService.EnsureAllTradingWindowsActiveAsync();

            var actual = await db.TradingWindows.CountAsync(x => !x.IsClosed);

            Assert.Equal(expected, actual);
        }

        private async Task SeedTradingWindowsAsync()
        {
            var db = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            await db.TradingWindows.AddRangeAsync(new TradingWindow
            {
                Start = DateTime.UtcNow,
                End = DateTime.UtcNow.AddMinutes(30),
                Duration = TimeSpan.FromMinutes(30),
                IsClosed = false,
            }, new TradingWindow
            {
                Start = DateTime.UtcNow.AddMinutes(-40),
                End = DateTime.UtcNow.AddMinutes(-10),
                Duration = TimeSpan.FromMinutes(30),
                IsClosed = false,
            });

            await db.SaveChangesAsync();

            db.ChangeTracker.Clear();
        }

        private async Task SeedBettingOptionPresetsAsync()
        {
            var db = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            await db.BettingOptionPresets.AddRangeAsync(new BettingOptionPreset
            {
                BarrierStep = 0.5m,
                BarrierCount = 5,
                CurrencyPair = new CurrencyPair
                {
                    CurrencyFrom = new Currency
                    {
                        Name = "USD"
                    },
                    CurrencyTo = new Currency
                    {
                        Name = "JPY"
                    },
                    Display = true,
                },
                TradingWindowPreset = new TradingWindowPreset
                {
                    Duration = TimeSpan.FromMinutes(30),
                }
            });

            await db.SaveChangesAsync();

            db.ChangeTracker.Clear();
        }
    }
}
