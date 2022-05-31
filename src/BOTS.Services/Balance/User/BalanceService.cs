namespace BOTS.Services.Balance.User
{
    using BOTS.Data.Infrastructure.Transactions;
    using BOTS.Services.Balance.System;
    using BOTS.Services.Balance.User.Events;
    using BOTS.Services.Common;
    using BOTS.Services.Infrastructure.Events;

    using global::System.Data;

    [TransientService]
    public class BalanceService : IBalanceService
    {
        private readonly IRepository<ApplicationUser> userRepository;
        private readonly ITreasuryService treasuryService;
        private readonly IEventManager<UpdateBalanceEvent> balanceEventManager;
        private readonly ITransactionManager transactionManager;

        public BalanceService(
            IRepository<ApplicationUser> userRepository,
            ITreasuryService treasuryService,
            IEventManager<UpdateBalanceEvent> balanceEventManager,
            ITransactionManager transactionManager)
        {
            this.userRepository = userRepository;
            this.treasuryService = treasuryService;
            this.balanceEventManager = balanceEventManager;
            this.transactionManager = transactionManager;
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

        public async Task DepositAsync(Guid userId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException(string.Format("{0} cannot be zero or negative number", nameof(amount)), nameof(amount));
            }

            var transaction =
                await this.transactionManager.BeginTransactionAsync(IsolationLevel.RepeatableRead);

            try
            {
                await this.AddToBalanceAsync(userId, amount);

                await this.treasuryService.AddSystemBalanceAsync(amount);
                await this.treasuryService.AddUserProfitsAsync(amount);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task WithdrawAsync(Guid userId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException(string.Format("{0} cannot be zero or negative number", nameof(amount)), nameof(amount));
            }

            var transaction =
                await this.transactionManager.BeginTransactionAsync(IsolationLevel.RepeatableRead);

            try
            {
                await this.SubtractFromBalanceAsync(userId, amount);

                await this.treasuryService.SubtractSystemBalanceAsync(amount);
                await this.treasuryService.SubtractUserProfitsAsync(amount);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        public async Task<bool> HasEnoughBalanceAsync(Guid userId, decimal amount)
            => await this.userRepository
                            .AllAsNoTracking()
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
