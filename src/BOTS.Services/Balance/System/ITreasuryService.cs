namespace BOTS.Services.Balance.System
{
    public interface ITreasuryService
    {
        Task AddSystemBalanceAsync(decimal amount);

        Task SubtractSystemBalanceAsync(decimal amount);

        Task AddUserProfitsAsync(decimal amount);

        Task SubtractUserProfitsAsync(decimal amount);

        Task<bool> CanPlaceBetAsync(decimal entryFee, decimal payout);
    }
}
