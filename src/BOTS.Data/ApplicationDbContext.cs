namespace BOTS.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using Models;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } = default!;

        public DbSet<Nationality> Nationalities { get; set; } = default!;

        public DbSet<Currency> Currencies { get; set; } = default!;

        public DbSet<CurrencyPair> CurrencyPairs { get; set; } = default!;

        public DbSet<TradingWindow> TradingWindows { get; set; } = default!;

        public DbSet<TradingWindowPreset> TradingWindowPresets { get; set; } = default!;

        public DbSet<BettingOptionPreset> BettingOptionPresets { get; set; } = default!;

        public DbSet<UserPreset> UserPresets { get; set; } = default!;

        public DbSet<BettingOption> BettingOptions { get; set; } = default!;

        public DbSet<Bet> Bets { get; set; } = default!;

        public DbSet<Treasury> Treasuries { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(builder);
        }
    }
}
