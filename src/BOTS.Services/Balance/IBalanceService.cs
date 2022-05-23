namespace BOTS.Services.Balance
{
    public interface IBalanceService
    {
        Task<decimal> GetBalanceAsync(Guid userId);

        Task<bool> AddToBalanceAsync(Guid userId, decimal amount);

        Task<bool> SubtractFromBalanceAsync(Guid userId, decimal amount);
    }
}
