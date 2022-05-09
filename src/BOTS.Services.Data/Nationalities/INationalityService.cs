namespace BOTS.Services.Data.Nationalities
{
    public interface INationalityService
    {
        Task<IEnumerable<T>> GetAllNationalitiesAsync<T>();

        Task<bool> NationalityExistsAsync(int nationalityId);
    }
}
