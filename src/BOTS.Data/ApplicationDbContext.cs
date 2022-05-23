namespace BOTS.Data
{
    using BOTS.Common;

    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using Models;

    using System.Text.Json;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        // TODO: on first initialization (maybe static constructor???) register entitiytypeconfiguration

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


        // TODO: maybe extract to entity type configuration

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<BettingOption>()
                .HasIndex(x => new { x.CurrencyPairId, x.TradingWindowId })
                .IsUnique();

            builder
                .Entity<BettingOption>()
                .Property(x => x.Barriers)
                .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null),
                                  x => JsonSerializer.Deserialize<decimal[]>(x, (JsonSerializerOptions?)null)!);

            builder
                .Entity<BettingOption>()
                .Property(x => x.CloseValue)
                .HasPrecision(GlobalConstants.BarrierDigitPrecision, GlobalConstants.DecimalPlacePrecision);

            builder
                .Entity<Bet>()
                .Property(x => x.BarrierPrediction)
                .HasPrecision(GlobalConstants.BarrierDigitPrecision, GlobalConstants.DecimalPlacePrecision);

            builder
                .Entity<BettingOptionPreset>()
                .HasKey(x => new { x.TradingWindowPresetId, x.CurrencyPairId });

            builder
                .Entity<ApplicationSetting>()
                .HasIndex(x => x.Key)
                .IsUnique();

            builder
                .Entity<CurrencyPair>()
                .HasIndex(x => new { x.CurrencyFromId, x.CurrencyToId })
                .IsUnique();

            builder
                .Entity<CurrencyPair>()
                .HasOne(x => x.CurrencyFrom)
                .WithMany(x => x.CurrenciesFrom)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<CurrencyPair>()
                .HasOne(x => x.CurrencyTo)
                .WithMany(x => x.CurrenciesTo)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .Entity<TradingWindowPreset>()
                .Property(x => x.Duration)
                .HasConversion<long>();

            base.OnModelCreating(builder);
        }
    }
}
