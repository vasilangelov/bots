namespace BOTS.Services.Balance.System
{
    using BOTS.Services.Common;

    [TransientService]
    public class TreasuryService : ITreasuryService
    {
        private readonly IRepository<Treasury> treasuryRepository;

        public TreasuryService(IRepository<Treasury> treasuryRepository)
        {
            this.treasuryRepository = treasuryRepository;
        }

        public async Task AddSystemBalanceAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number");
            }

            var treasury = await this.treasuryRepository.All().FirstAsync();

            treasury.SystemBalance += amount;

            await this.treasuryRepository.SaveChangesAsync();
        }

        public async Task SubtractSystemBalanceAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number");
            }

            var treasury = await this.treasuryRepository.All().FirstAsync();

            var updatedBalance = treasury.SystemBalance - amount;

            if (updatedBalance < 0)
            {
                throw new InvalidOperationException("Cannot subtract amount from system balance");
            }

            treasury.SystemBalance = updatedBalance;

            await this.treasuryRepository.SaveChangesAsync();
        }

        public async Task AddUserProfitsAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number");
            }

            var treasury = await this.treasuryRepository.All().FirstAsync();

            var updatedProfits = treasury.UserProfits + amount;

            if (updatedProfits > treasury.SystemBalance)
            {
                throw new InvalidOperationException("Exceeded user profits");
            }

            treasury.UserProfits = updatedProfits;

            await this.treasuryRepository.SaveChangesAsync();
        }

        public async Task SubtractUserProfitsAsync(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be a positive number");
            }

            var treasury = await this.treasuryRepository.All().FirstAsync();

            var updatedProfits = treasury.UserProfits - amount;

            if (updatedProfits < 0)
            {
                throw new InvalidOperationException("Cannot subtract amount from user profits");
            }

            treasury.UserProfits = updatedProfits;

            await this.treasuryRepository.SaveChangesAsync();
        }

        public async Task<bool> CanPlaceBetAsync(decimal entryFee, decimal payout)
            => await this.treasuryRepository
                            .AllAsNoTracking()
                            .AnyAsync(x => x.SystemBalance + entryFee >= x.UserProfits + payout);
    }
}
