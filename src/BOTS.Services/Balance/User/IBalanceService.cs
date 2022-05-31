namespace BOTS.Services.Balance
{
    public interface IBalanceService
    {
        Task DepositAsync(Guid userId, decimal amount);

        Task WithdrawAsync(Guid userId, decimal amount);

        Task AddToBalanceAsync(Guid userId, decimal amount);

        Task SubtractFromBalanceAsync(Guid userId, decimal amount);

        Task<bool> HasEnoughBalanceAsync(Guid userId, decimal amount);
    }
}
