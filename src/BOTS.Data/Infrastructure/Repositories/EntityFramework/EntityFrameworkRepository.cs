namespace BOTS.Data.Infrastructure.Repositories.EntityFramework
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using BOTS.Data.Infrastructure.Repositories;

    using Microsoft.EntityFrameworkCore;

    public class EntityFrameworkRepository<T> : IRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DbSet<T> dbSet;

        public EntityFrameworkRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<T>();
        }

        public IQueryable<T> All()
            => this.dbSet;

        public IQueryable<T> AllAsNoTracking()
            => this.dbSet.AsNoTracking();

        public async Task<T?> GetById(object id)
            => await this.dbSet.FindAsync(id);

        public async Task AddAsync(T item)
            => await this.dbSet.AddAsync(item);

        public void Update(T item)
        {
            var entry = this.dbContext.Entry(item);

            if (entry.State == EntityState.Detached)
            {
                this.dbSet.Attach(item);
            }

            entry.State = EntityState.Modified;
        }

        public void Patch(T item, params Expression<Func<T, object>>[] includeProperties)
        {
            var entry = this.dbContext.Entry(item);

            if (entry.State == EntityState.Detached)
            {
                this.dbSet.Attach(item);
            }

            foreach (var includeProperty in includeProperties)
            {
                entry.Property(includeProperty).IsModified = true;
            }
        }

        public void Remove(T item)
            => this.dbSet.Remove(item);

        public async Task<int> SaveChangesAsync()
        {
            var rowsAffected = await this.dbContext.SaveChangesAsync();

            this.dbContext.ChangeTracker.Clear();

            return rowsAffected;
        }

        public int SaveChanges()
        {
            var rowsAffected = this.dbContext.SaveChanges();

            this.dbContext.ChangeTracker.Clear();

            return rowsAffected;
        }

        public void Dispose()
        {
            this.dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
