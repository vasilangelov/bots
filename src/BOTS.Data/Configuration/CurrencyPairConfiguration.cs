namespace BOTS.Data.Configuration
{
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class CurrencyPairConfiguration : IEntityTypeConfiguration<CurrencyPair>
    {
        public void Configure(EntityTypeBuilder<CurrencyPair> builder)
        {
            builder
                .HasIndex(x => new { x.CurrencyFromId, x.CurrencyToId })
                .IsUnique();

            builder
                .HasOne(x => x.CurrencyFrom)
                .WithMany(x => x.CurrenciesFrom)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(x => x.CurrencyTo)
                .WithMany(x => x.CurrenciesTo)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
