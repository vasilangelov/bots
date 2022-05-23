namespace BOTS.Services.Balance
{
    using BOTS.Services.Common;

    [TransientService]
    public class BalanceService : IBalanceService
    {
        private readonly IRepository<ApplicationUser> userRepository;

        public BalanceService(IRepository<ApplicationUser> userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<bool> AddToBalanceAsync(Guid userId, decimal amount)
        {
            var user = await this.GetUserAsync(userId);

            if (user is null)
            {
                return false;
            }

            decimal newBalance = user.Balance + amount;

            await this.UpdateUserBalanceAsync(user, newBalance);

            return true;
        }

        public async Task<bool> SubtractFromBalanceAsync(Guid userId, decimal amount)
        {
            var user = await this.GetUserAsync(userId);

            if (user is null || user.Balance < amount)
            {
                return false;
            }

            decimal newBalance = user.Balance - amount;

            await this.UpdateUserBalanceAsync(user, newBalance);

            return true;
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
            => await this.userRepository
                            .AllAsNotracking()
                            .Where(x => x.Id == userId)
                            .Select(x => x.Balance)
                            .FirstAsync();

        private async Task<ApplicationUser?> GetUserAsync(Guid userId)
            => await this.userRepository
                            .AllAsNotracking()
                            .FirstOrDefaultAsync(x => x.Id == userId);

        private async Task UpdateUserBalanceAsync(ApplicationUser user, decimal balance)
        {
            user.Balance = balance;

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            // TODO: notify user on update???
        }
    }
}
