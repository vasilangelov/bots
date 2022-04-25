namespace BOTS.Services.Data.Common
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    using BOTS.Data;

    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DbSet<T> dbSet;

        public Repository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<T>();
        }

        public IQueryable<T> All()
            => this.dbSet;

        public IQueryable<T> AllAsNotracking()
            => this.dbSet.AsNoTracking();

        public async Task AddAsync(T item)
            => await this.dbSet.AddAsync(item);

        public void Update(T item)
            => this.dbSet.Update(item);

        public void Remove(T item)
            => this.dbSet.Remove(item);

        public async Task<int> SaveChangesAsync()
            => await this.dbContext.SaveChangesAsync();
    }
}
