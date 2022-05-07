﻿namespace BOTS.Services.Data.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<ApplicationUser> userRepository;

        public UserService(IRepository<ApplicationUser> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<bool> HasActiveBetForCurrencyPairAsync(string userId, int currencyPairId)
            => await this.userRepository
                .AllAsNotracking()
                .AnyAsync(x => x.Id == userId &&
                          x.Bets.Any(y => y.TradingWindow.CurrencyPairId == currencyPairId &&
                          DateTime.UtcNow < y.TradingWindow.End));

        public async Task<bool> SubtractFromBalanceAsync(string userId, decimal amount)
        {
            var user = await this.userRepository
                                    .AllAsNotracking()
                                    .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null || user.Balance < amount)
            {
                return false;
            }

            user.Balance -= amount;

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<decimal> GetUserBalance(string userId)
            => await this.userRepository
                            .AllAsNotracking()
                            .Where(x => x.Id == userId)
                            .Select(x => x.Balance)
                            .FirstAsync();
    }
}
