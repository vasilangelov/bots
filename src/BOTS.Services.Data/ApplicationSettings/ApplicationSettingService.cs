namespace BOTS.Services.Data.ApplicationSettings
{
    using BOTS.Data.Repositories;
    using System.Text.Json;

    public class ApplicationSettingService : IApplicationSettingService
    {
        private const string InvalidConversionExceptionMessage = "Cannot convert value to type {0}";

        private readonly IRepository<ApplicationSetting> applicationSettingRepository;

        public ApplicationSettingService(IRepository<ApplicationSetting> applicationSettingRepository)
        {
            this.applicationSettingRepository = applicationSettingRepository;
        }

        public async Task<T> GetValue<T>(string key)
        {
            string value = await this.applicationSettingRepository
                                        .AllAsNotracking()
                                        .Where(x => x.Key == key)
                                        .Select(x => x.Value)
                                        .FirstAsync();

            T? result = JsonSerializer.Deserialize<T>(value);

            if (result is null)
            {
                throw new InvalidOperationException(string.Format(InvalidConversionExceptionMessage, typeof(T).FullName));
            }

            return result;
        }
    }
}
