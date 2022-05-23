namespace BOTS.Data.Infrastructure.Transactions.EntityFramework
{
    using Microsoft.EntityFrameworkCore.Storage;

    internal class EntityFrameworkTransactionContext : ITransactionContext
    {
        public static EntityFrameworkTransactionContext FromDbContextTransaction(
            IDbContextTransaction dbContextTransaction)
        {
            if (dbContextTransaction is null)
            {
                throw new ArgumentNullException(nameof(dbContextTransaction));
            }

            return new EntityFrameworkTransactionContext(dbContextTransaction);
        }

        private readonly IDbContextTransaction dbContextTransaction;

        public EntityFrameworkTransactionContext(IDbContextTransaction dbContextTransaction)
        {
            this.dbContextTransaction = dbContextTransaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => this.dbContextTransaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => this.dbContextTransaction.RollbackAsync(cancellationToken);

        public void Dispose()
        {
            this.dbContextTransaction.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await this.dbContextTransaction.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
