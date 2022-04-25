namespace BOTS.Services.Data.Common
{
    public interface IRepository<T>
        where T : class
    {
        IQueryable<T> All();

        IQueryable<T> AllAsNotracking();

        Task AddAsync(T item);

        void Update(T item);

        void Remove(T item);

        Task<int> SaveChangesAsync();
    }
}
