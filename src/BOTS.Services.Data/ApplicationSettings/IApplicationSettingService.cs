namespace BOTS.Services.Data.ApplicationSettings
{
    public interface IApplicationSettingService
    {
        Task<T> GetValue<T>(string key);
    }
}
