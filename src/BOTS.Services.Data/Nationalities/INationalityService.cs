namespace BOTS.Services.Data.Nationalities
{
    public interface INationalityService
    {
        Task<IEnumerable<T>> GetAllNationalitiesAsync<T>(CancellationToken cancellationToken = default);

        Task<bool> NationalityExistsAsync(int nationalityId, CancellationToken cancellationToken = default);
    }
}
