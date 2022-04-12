namespace BOTS.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;

    using BOTS.Data;

    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly ApplicationDbContext dbContext;
        private readonly DbSet<T> set;

        public Repository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.set = this.dbContext.Set<T>();
        }

        public IQueryable<T> All()
            => this.set;

        public IQueryable<T> AllAsNotracking()
            => this.set.AsNoTracking();

        public async Task AddAsync(T item)
            => await this.set.AddAsync(item);

        public void Update(T item)
            => this.set.Update(item);

        public void Remove(T item)
            => this.set.Remove(item);

        public async Task<int> SaveChangesAsync()
            => await this.dbContext.SaveChangesAsync();
    }
}
