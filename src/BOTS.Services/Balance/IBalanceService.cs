namespace BOTS.Services.Balance
{
    public interface IBalanceService
    {
        Task AddToBalanceAsync(Guid userId, decimal amount);

        Task SubtractFromBalanceAsync(Guid userId, decimal amount);

        Task<bool> HasEnoughBalanceAsync(Guid userId, decimal amount);
    }
}
