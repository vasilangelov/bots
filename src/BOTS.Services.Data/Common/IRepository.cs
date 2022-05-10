namespace BOTS.Services.Data.Common
{
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public interface IRepository<T>
        where T : class
    {
        IQueryable<T> All();

        IQueryable<T> AllAsNotracking();

        Task AddAsync(T item);

        public EntityEntry<T> Update(T item);

        void Remove(T item);

        Task<int> SaveChangesAsync();
    }
}
