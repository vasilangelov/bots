namespace BOTS.Services.Data.Nationalities
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.EntityFrameworkCore;

    public class NationalityService : INationalityService
    {
        private readonly IRepository<Nationality> nationalityRepository;
        private readonly IMapper mapper;

        public NationalityService(IRepository<Nationality> nationalityRepository, IMapper mapper)
        {
            this.nationalityRepository = nationalityRepository;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<T>> GetAllNationalitiesAsync<T>(CancellationToken cancellationToken = default)
            => await this.nationalityRepository
                        .AllAsNotracking()
                        .ProjectTo<T>(this.mapper.ConfigurationProvider)
                        .ToArrayAsync(cancellationToken);

        public async Task<bool> NationalityExistsAsync(int nationalityId, CancellationToken cancellationToken = default)
            => await this.nationalityRepository
                        .AllAsNotracking()
                        .AnyAsync(x => x.Id == nationalityId, cancellationToken);
    }
}
