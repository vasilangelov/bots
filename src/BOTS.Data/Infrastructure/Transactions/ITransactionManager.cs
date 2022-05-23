namespace BOTS.Data.Infrastructure.Transactions
{
    using System.Data;

    public interface ITransactionManager
    {
        Task<ITransactionContext> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default);
    }
}
