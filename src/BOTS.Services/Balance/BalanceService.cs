namespace BOTS.Services.Balance
{
    using BOTS.Services.Balance.Events;
    using BOTS.Services.Common;
    using BOTS.Services.Infrastructure.Events;

    [TransientService]
    public class BalanceService : IBalanceService
    {
        private readonly IRepository<ApplicationUser> userRepository;
        private readonly IEventManager<UpdateBalanceEvent> balanceEventManager;

        public BalanceService(IRepository<ApplicationUser> userRepository, IEventManager<UpdateBalanceEvent> balanceEventManager)
        {
            this.userRepository = userRepository;
            this.balanceEventManager = balanceEventManager;
        }

        public async Task AddToBalanceAsync(Guid userId, decimal amount)
        {
            var user = await this.GetUserAsync(userId);

            if (user is null)
            {
                throw new InvalidOperationException("User with id does not exist");
            }

            decimal newBalance = user.Balance + amount;

            await this.UpdateUserBalanceAsync(user, newBalance);
        }

        public async Task SubtractFromBalanceAsync(Guid userId, decimal amount)
        {
            var user = await this.GetUserAsync(userId);

            if (user is null || user.Balance < amount)
            {
                throw new InvalidOperationException("Cannot subtract from balance");
            }

            decimal newBalance = user.Balance - amount;

            await this.UpdateUserBalanceAsync(user, newBalance);
        }

        public async Task<bool> HasEnoughBalanceAsync(Guid userId, decimal amount)
            => await this.userRepository
                            .AllAsNotracking()
                            .AnyAsync(x => x.Id == userId && x.Balance >= amount);

        private async Task<ApplicationUser?> GetUserAsync(Guid userId)
            => await this.userRepository.GetById(userId);

        private async Task UpdateUserBalanceAsync(ApplicationUser user, decimal balance)
        {
            user.Balance = balance;

            this.userRepository.Update(user);
            await this.userRepository.SaveChangesAsync();

            var eventContext = new UpdateBalanceEvent { UserId = user.Id, Balance = balance };

            await this.balanceEventManager.EmitAsync(eventContext);
        }
    }
}
