namespace BOTS.Services.UserPresets
{
    public interface IUserPresetService
    {
        Task<Guid> AddPresetAsync<T>(Guid userId, T model);

        Task UpdatePresetAsync<T>(T model);

        Task DeletePresetAsync(Guid presetId);

        Task SetDefaultPresetAsync(Guid userId, Guid presetId);

        Task<Guid?> GetActiveUserPresetIdAsync(Guid userId);

        Task<bool> IsUserPresetOwnerAsync(Guid userId, Guid presetId);

        Task<T> GetActiveUserPresetAsync<T>(Guid userId);

        Task<T> GetActiveUserPresetOrDefaultAsync<T>(Guid userId);

        Task<T> GetUserPresetAsync<T>(Guid presetId);

        Task<IEnumerable<T>> GetUserPresetsAsync<T>(Guid userId);
    }
}
