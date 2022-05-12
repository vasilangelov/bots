namespace BOTS.Services.Data.UserPresets
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using BOTS.Services.Data.ApplicationSettings;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class UserPresetService : IUserPresetService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRepository<UserPreset> userPresetRepository;
        private readonly IMapper mapper;

        public UserPresetService(
            IHttpContextAccessor httpContextAccessor,
            IRepository<UserPreset> userPresetRepository,
            IMapper mapper)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userPresetRepository = userPresetRepository;
            this.mapper = mapper;
        }

        public async Task<string> AddPresetAsync<T>(string userId, T model)
        {
            UserPreset preset = this.mapper.Map<UserPreset>(model);

            preset.OwnerId = userId;

            var userHasActivePreset = await this.UserHasActivePresetAsync(userId);

            if (!userHasActivePreset)
            {
                preset.IsActive = true;
            }

            await this.userPresetRepository.AddAsync(preset);

            await this.userPresetRepository.SaveChangesAsync();

            return preset.Id;
        }

        public async Task<T> GetActiveUserPresetAsync<T>(string userId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .Where(x => x.OwnerId == userId && x.IsActive)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .FirstAsync();

        public async Task<string?> GetActiveUserPresetIdAsync(string userId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .Where(x => x.OwnerId == userId && x.IsActive)
                            .Select(x => x.Id)
                            .FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetUserPresetsAsync<T>(string userId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .Where(x => x.OwnerId == userId)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .ToArrayAsync();

        public async Task<T> GetUserPresetAsync<T>(string presetId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .Where(x => x.Id == presetId)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .FirstAsync();

        public async Task<bool> IsUserPresetOwnerAsync(string userId, string presetId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .AnyAsync(x => x.OwnerId == userId && x.Id == presetId);

        public async Task SetDefaultPresetAsync(string userId, string presetId)
        {
            var oldPreset = await this.GetActiveUserPresetAsync(userId);

            if (oldPreset is not null)
            {
                oldPreset.IsActive = false;

                this.userPresetRepository.Update(oldPreset);
            }

            var newPreset = await this.GetUserPresetAsync(userId, presetId);

            if (newPreset is null)
            {
                throw new InvalidOperationException("User does not have preset with this id");
            }

            newPreset.IsActive = true;

            this.userPresetRepository.Update(newPreset);

            await this.userPresetRepository.SaveChangesAsync();
        }

        public async Task UpdatePresetAsync<T>(T model)
        {
            UserPreset preset = this.mapper.Map<UserPreset>(model);

            var entityEntry = this.userPresetRepository.Update(preset);

            entityEntry.Property(x => x.OwnerId).IsModified = false;
            entityEntry.Property(x => x.IsActive).IsModified = false;

            await this.userPresetRepository.SaveChangesAsync();
        }

        public async Task SetPresetActiveAsync(string userId, string presetId)
        {
            UserPreset? oldActivePreset = await this.GetActiveUserPresetAsync(userId);

            if (oldActivePreset is not null)
            {
                oldActivePreset.IsActive = false;

                this.userPresetRepository.Update(oldActivePreset);
            }

            var newActivePreset = await this.GetUserPresetAsync(userId, presetId);

            if (newActivePreset is null)
            {
                throw new ArgumentException("User does not have a preset with this id", nameof(presetId));
            }

            newActivePreset.IsActive = true;

            this.userPresetRepository.Update(newActivePreset);

            await this.userPresetRepository.SaveChangesAsync();
        }

        public async Task DeletePresetAsync(string presetId)
        {
            UserPreset? preset = await this.GetPresetAsync(presetId);

            if (preset is null)
            {
                throw new ArgumentException("Invalid preset id", presetId);
            }

            if (preset.IsActive)
            {
                UserPreset? newActive = await this.GetFirstUserPreset(preset.OwnerId, presetId);

                if (newActive is not null)
                {
                    newActive.IsActive = true;

                    this.userPresetRepository.Update(newActive);
                }
            }

            this.userPresetRepository.Remove(preset);

            await this.userPresetRepository.SaveChangesAsync();
        }

        public async Task<T> GetActiveUserPresetOrDefaultAsync<T>(string userId)
        {
            UserPreset? userPreset = await this.GetActiveUserPresetAsync(userId);

            if (userPreset is null)
            {
                var settingService = this.httpContextAccessor
                                            .HttpContext
                                            .RequestServices
                                            .GetRequiredService<IApplicationSettingService>();

                userPreset = await settingService.GetValue<UserPreset>("DefaultUserPreset");
            }

            return this.mapper.Map<T>(userPreset);
        }

        private async Task<bool> UserHasActivePresetAsync(string userId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .AnyAsync(x => x.OwnerId == userId && x.IsActive);

        private async Task<UserPreset?> GetActiveUserPresetAsync(string userId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.IsActive);

        private async Task<UserPreset?> GetUserPresetAsync(string userId, string presetId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.Id == presetId);

        private async Task<UserPreset?> GetPresetAsync(string presetId)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .FirstOrDefaultAsync(x => x.Id == presetId);

        private async Task<UserPreset?> GetFirstUserPreset(string userId, string differentFrom)
            => await this.userPresetRepository
                            .AllAsNotracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.Id != differentFrom);
    }
}
