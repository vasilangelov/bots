namespace BOTS.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using Models;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Nationality> Nationalities { get; set; } = default!;

        public DbSet<Currency> Currencies { get; set; } = default!;

        public DbSet<CurrencyPair> CurrencyPairs { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CurrencyPair>()
                .HasKey(c => new { c.LeftId, c.RightId });

            builder.Entity<CurrencyPair>()
                .HasOne(x => x.Left)
                .WithMany(x => x.LeftPairs)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CurrencyPair>()
                .HasOne(x => x.Right)
                .WithMany(x => x.RightPairs)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(builder);
        }
    }
}
