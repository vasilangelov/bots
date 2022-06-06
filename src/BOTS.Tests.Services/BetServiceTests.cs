namespace BOTS.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BOTS.Data;
    using BOTS.Data.Models;
    using BOTS.Services.Currencies.CurrencyRates;
    using BOTS.Services.Trades.Bets;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    using Xunit;

    public class BetServiceTests : TestBase
    {
        [Fact]
        public async Task ShouldPlaceBetSuccessfuly()
        {
            var userId = await this.SeedUserAsync(10000);
            await this.SeedTreasuryAsync(20000, 10000);
            var barriers = await this.GenerateBarriersAsync();
            var bettingOptionId = await this.SeedBettingOptionAsync(TimeSpan.FromMinutes(30), barriers);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            var exception = await Record.ExceptionAsync(async () =>
                await betService.PlaceBetAsync(userId, bettingOptionId, BetType.Higher, barriers[2], 10));

            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldThrowWhenInvalidBarrier()
        {
            var userId = await this.SeedUserAsync(10000);
            await this.SeedTreasuryAsync(20000, 10000);
            var barriers = await this.GenerateBarriersAsync();
            var bettingOptionId = await this.SeedBettingOptionAsync(TimeSpan.FromMinutes(30), barriers);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await betService.PlaceBetAsync(userId, bettingOptionId, BetType.Higher, -1, 10));
        }

        [Fact]
        public async Task ShouldThrowWhenNoUserBalance()
        {
            var userId = await this.SeedUserAsync(0);
            await this.SeedTreasuryAsync(20000, 0);
            var barriers = await this.GenerateBarriersAsync();
            var bettingOptionId = await this.SeedBettingOptionAsync(TimeSpan.FromMinutes(30), barriers);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await betService.PlaceBetAsync(userId, bettingOptionId, BetType.Higher, barriers[2], 10));
        }

        [Fact]
        public async Task ShouldThrowWhenNoSystemBalance()
        {
            var userId = await this.SeedUserAsync(10000);
            await this.SeedTreasuryAsync(10000, 10000);
            var barriers = await this.GenerateBarriersAsync();
            var bettingOptionId = await this.SeedBettingOptionAsync(TimeSpan.FromMinutes(30), barriers);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await betService.PlaceBetAsync(userId, bettingOptionId, BetType.Higher, barriers[2], 10));
        }

        [Fact]
        public async Task ShouldThrowWhenBetForCurrencyPairExists()
        {
            var userId = await this.SeedUserAsync(10000);
            await this.SeedTreasuryAsync(20000, 10000);
            var barriers = await this.GenerateBarriersAsync();
            var bettingOption1Id = await this.SeedBettingOptionAsync(TimeSpan.FromMinutes(30), barriers);

            var bettingOption2Id = await this.SeedBettingOptionAsync(TimeSpan.FromHours(1), barriers);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await betService.PlaceBetAsync(userId, bettingOption1Id, BetType.Higher, barriers[2], 10);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await betService.PlaceBetAsync(userId, bettingOption2Id, BetType.Higher, barriers[2], 10));
        }

        [Fact]
        public async Task ShouldPayoutWinningBetsAndUpdateUserBalance()
        {
            decimal userBalance = 10000;

            var userId = await this.SeedUserAsync(userBalance);
            await this.SeedTreasuryAsync(20000, userBalance);

            decimal payout = 10;

            var tradingWindowId = await this.SeedEndedTradingWindowAsync(userId, BetType.Higher, 2, 1, payout);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await betService.PayoutBetsAsync(tradingWindowId);

            decimal expected = userBalance + payout;
            decimal actual = await this.GetUserBalanceAsync(userId);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ShouldNotPayoutLosingBets()
        {
            decimal userBalance = 10000;

            var userId = await this.SeedUserAsync(userBalance);
            await this.SeedTreasuryAsync(20000, userBalance);

            decimal payout = 10;

            var tradingWindowId = await this.SeedEndedTradingWindowAsync(userId, BetType.Higher, 1, 2, payout);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await betService.PayoutBetsAsync(tradingWindowId);

            decimal expected = userBalance;
            decimal actual = await this.GetUserBalanceAsync(userId);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task LosingBetsShouldBeSubtractedFromUserProfits()
        {
            decimal userBalance = 10000;
            decimal systemBalance = 20000;

            var userId = await this.SeedUserAsync(userBalance);
            await this.SeedTreasuryAsync(systemBalance, userBalance);

            decimal payout = 10;
            decimal entryFee = 5;

            var tradingWindowId = await this.SeedEndedTradingWindowAsync(userId, BetType.Higher, 1, 2, payout, entryFee);

            var betService = this.serviceProvider.GetRequiredService<IBetService>();

            await betService.PayoutBetsAsync(tradingWindowId);

            decimal expected = userBalance - (payout - entryFee);
            decimal actual = await this.GetUserProfitsAsync();

            Assert.Equal(expected, actual);
        }

        private async Task<decimal> GetUserProfitsAsync()
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await dbContext.Treasuries.Select(x => x.UserProfits).FirstAsync();
        }

        private async Task<decimal> GetUserBalanceAsync(Guid userId)
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            return await dbContext.Users.Where(x => x.Id == userId).Select(x => x.Balance).FirstAsync();
        }

        private async Task<Guid> SeedEndedTradingWindowAsync(
            Guid userId,
            BetType betType,
            decimal closeValue,
            decimal prediction,
            decimal payout,
            decimal entryFee = 0)
        {
            var tradingWindow = new TradingWindow
            {
                IsClosed = true,
                BettingOptions = new BettingOption[]
                {
                    new()
                    {
                        Barriers = Array.Empty<decimal>(),
                        CloseValue = closeValue,
                        Bets = new Bet[]
                        {
                            new()
                            {
                                EntryFee = entryFee,
                                Payout = payout,
                                UserId = userId,
                                Type = betType,
                                BarrierPrediction = prediction,
                            }
                        }
                    }
                }
            };

            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Add(tradingWindow);
            await dbContext.SaveChangesAsync();

            return tradingWindow.Id;
        }

        private async Task<decimal[]> GenerateBarriersAsync()
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var currencyPair = new CurrencyPair
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
            };

            dbContext.Add(currencyPair);
            await dbContext.SaveChangesAsync();

            var currencyRateProviderService = this.serviceProvider.GetRequiredService<ICurrencyRateProviderService>();

            var currentCurrencyRate = await currencyRateProviderService.GetCurrencyRateAsync(1);

            return BarrierActions.GenerateBarriers(5, currentCurrencyRate, 0.5m);
        }

        private async Task<Guid> SeedUserAsync(decimal balance)
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var user = new ApplicationUser
            {
                Balance = balance,
            };

            dbContext.Add(user);

            await dbContext.SaveChangesAsync();

            return user.Id;
        }

        private async Task SeedTreasuryAsync(decimal systemBalance, decimal userProfits)
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var treasury = new Treasury
            {
                SystemBalance = systemBalance,
                UserProfits = userProfits,
            };

            dbContext.Add(treasury);

            await dbContext.SaveChangesAsync();
        }

        private async Task<Guid> SeedBettingOptionAsync(TimeSpan windowDuration, decimal[] barriers)
        {
            var dbContext = this.serviceProvider.GetRequiredService<ApplicationDbContext>();

            var bettingOption = new BettingOption
            {
                TradingWindow = new()
                {
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.Add(windowDuration),
                    Duration = windowDuration,
                    IsClosed = false,
                },
                BarrierStep = 0.5m,
                Barriers = barriers,
                CurrencyPairId = 1,
                CloseValue = null,
            };

            dbContext.Add(bettingOption);

            await dbContext.SaveChangesAsync();

            return bettingOption.Id;
        }
    }
}
