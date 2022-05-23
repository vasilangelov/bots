namespace BOTS.Data.Infrastructure.Transactions.EntityFramework
{
    using System.Data;

    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkTransactionManager : ITransactionManager
    {
        private readonly ApplicationDbContext dbContext;

        public EntityFrameworkTransactionManager(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ITransactionContext> BeginTransactionAsync(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default)
        {
            var dbContextTransaction = await this.dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);

            return EntityFrameworkTransactionContext.FromDbContextTransaction(dbContextTransaction);
        }
    }
}
