namespace BOTS.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using Models;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } = default!;

        public DbSet<Nationality> Nationalities { get; set; } = default!;

        public DbSet<Currency> Currencies { get; set; } = default!;

        public DbSet<CurrencyPair> CurrencyPairs { get; set; } = default!;

        public DbSet<TradingWindow> TradingWindows { get; set; } = default!;

        public DbSet<TradingWindowOption> TradingWindowOptions { get; set; } = default!;

        public DbSet<UserPreset> UserPresets { get; set; } = default!;

        public DbSet<Bet> Bets { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<ApplicationSetting>()
                .HasIndex(x => x.Key)
                .IsUnique();

            builder
                .Entity<CurrencyPair>()
                .HasIndex(x => new { x.LeftId, x.RightId })
                .IsUnique();

            builder
                .Entity<CurrencyPair>()
                .HasOne(x => x.Left)
                .WithMany(x => x.LeftPairs)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<CurrencyPair>()
                .HasOne(x => x.Right)
                .WithMany(x => x.RightPairs)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<TradingWindowOption>()
                .Property(x => x.Duration)
                .HasConversion<long>();

            base.OnModelCreating(builder);
        }
    }
}
