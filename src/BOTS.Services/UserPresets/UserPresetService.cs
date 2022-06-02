namespace BOTS.Services.UserPresets
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Services.ApplicationSettings;
    using BOTS.Services.Common;

    [TransientService]
    public class UserPresetService : IUserPresetService
    {
        private readonly IRepository<UserPreset> userPresetRepository;
        private readonly IApplicationSettingService applicationSettingService;
        private readonly IMapper mapper;

        public UserPresetService(
            IRepository<UserPreset> userPresetRepository,
            IApplicationSettingService applicationSettingService,
            IMapper mapper)
        {
            this.userPresetRepository = userPresetRepository;
            this.applicationSettingService = applicationSettingService;
            this.mapper = mapper;
        }

        public async Task<Guid> AddPresetAsync<T>(Guid userId, T model)
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

        public async Task<T> GetActiveUserPresetAsync<T>(Guid userId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .Where(x => x.OwnerId == userId && x.IsActive)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .FirstAsync();

        public async Task<Guid?> GetActiveUserPresetIdAsync(Guid userId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .Where(x => x.OwnerId == userId && x.IsActive)
                            .Select(x => x.Id as Guid?)
                            .FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetUserPresetsAsync<T>(Guid userId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .Where(x => x.OwnerId == userId)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .ToArrayAsync();

        public async Task<T> GetUserPresetAsync<T>(Guid presetId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .Where(x => x.Id == presetId)
                            .ProjectTo<T>(this.mapper.ConfigurationProvider)
                            .FirstAsync();

        public async Task<bool> IsUserPresetOwnerAsync(Guid userId, Guid presetId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .AnyAsync(x => x.OwnerId == userId && x.Id == presetId);

        public async Task SetDefaultPresetAsync(Guid userId, Guid presetId)
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

            this.userPresetRepository.Patch(
                preset,
                x => x.Name,
                x => x.CurrencyPairId,
                x => x.ChartType,
                x => x.Payout);

            await this.userPresetRepository.SaveChangesAsync();
        }

        public async Task DeletePresetAsync(Guid presetId)
        {
            UserPreset? preset = await this.GetPresetAsync(presetId);

            if (preset is null)
            {
                throw new ArgumentException("Invalid preset id", nameof(presetId));
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

        public async Task<T> GetActiveUserPresetOrDefaultAsync<T>(Guid userId)
        {
            UserPreset? userPreset = await this.GetActiveUserPresetAsync(userId);

            if (userPreset is null)
            {
                userPreset = await this.applicationSettingService.GetValueAsync<UserPreset>("DefaultUserPreset");
            }

            return this.mapper.Map<T>(userPreset);
        }

        private async Task<bool> UserHasActivePresetAsync(Guid userId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .AnyAsync(x => x.OwnerId == userId && x.IsActive);

        private async Task<UserPreset?> GetActiveUserPresetAsync(Guid userId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.IsActive);

        private async Task<UserPreset?> GetUserPresetAsync(Guid userId, Guid presetId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.Id == presetId);

        private async Task<UserPreset?> GetPresetAsync(Guid presetId)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == presetId);

        private async Task<UserPreset?> GetFirstUserPreset(Guid userId, Guid differentFrom)
            => await this.userPresetRepository
                            .AllAsNoTracking()
                            .FirstOrDefaultAsync(x => x.OwnerId == userId && x.Id != differentFrom);
    }
}
