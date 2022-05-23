namespace BOTS.Data.Infrastructure.Transactions
{
    public interface ITransactionContext : IDisposable, IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
