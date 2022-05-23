namespace BOTS.Services.ApplicationSettings
{
    public interface IApplicationSettingService
    {
        Task<T> GetValue<T>(string key);
    }
}
