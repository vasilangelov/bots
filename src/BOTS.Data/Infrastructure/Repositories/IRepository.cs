namespace BOTS.Data.Infrastructure.Repositories
{
    using System.Linq.Expressions;

    public interface IRepository<T> : IDisposable
        where T : class
    {
        IQueryable<T> All();

        IQueryable<T> AllAsNoTracking();

        Task<T?> GetById(object id);

        Task AddAsync(T item);

        void Update(T item);

        void Patch(T item, params Expression<Func<T, object>>[] includeProperties);

        void Remove(T item);

        int SaveChanges();

        Task<int> SaveChangesAsync();
    }
}
