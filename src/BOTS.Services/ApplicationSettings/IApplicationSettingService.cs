namespace BOTS.Services.ApplicationSettings
{
    public interface IApplicationSettingService
    {
        Task<T> GetValueAsync<T>(string key);
    }
}
