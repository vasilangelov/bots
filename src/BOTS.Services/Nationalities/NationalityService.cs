namespace BOTS.Services.Nationalities
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using BOTS.Services.Common;

    [TransientService]
    public class NationalityService : INationalityService
    {
        private readonly IRepository<Nationality> nationalityRepository;
        private readonly IMapper mapper;

        public NationalityService(IRepository<Nationality> nationalityRepository, IMapper mapper)
        {
            this.nationalityRepository = nationalityRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<T>> GetAllNationalitiesAsync<T>()
            => await this.nationalityRepository
                        .AllAsNoTracking()
                        .ProjectTo<T>(this.mapper.ConfigurationProvider)
                        .ToArrayAsync();

        public async Task<bool> NationalityExistsAsync(int nationalityId)
            => await this.nationalityRepository
                        .AllAsNoTracking()
                        .AnyAsync(x => x.Id == nationalityId);
    }
}
