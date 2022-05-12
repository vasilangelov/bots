namespace BOTS.Services.Data.UserPresets
{
    public interface IUserPresetService
    {
        Task<string> AddPresetAsync<T>(string userId, T model);

        Task UpdatePresetAsync<T>(T model);

        Task DeletePresetAsync(string presetId);

        Task SetDefaultPresetAsync(string userId, string presetId);

        Task SetPresetActiveAsync(string userId, string presetId);

        Task<IEnumerable<T>> GetUserPresetsAsync<T>(string userId);

        Task<T> GetUserPresetAsync<T>(string presetId);

        Task<string?> GetActiveUserPresetIdAsync(string userId);

        Task<T> GetActiveUserPresetAsync<T>(string userId);

        Task<T> GetActiveUserPresetOrDefaultAsync<T>(string userId);

        Task<bool> IsUserPresetOwnerAsync(string userId, string presetId);
    }
}
