namespace BOTS.Data.Configuration
{
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class TradingWindowPresetConfiguration : IEntityTypeConfiguration<TradingWindowPreset>
    {
        public void Configure(EntityTypeBuilder<TradingWindowPreset> builder)
        {
            builder
                .Property(x => x.Duration)
                .HasConversion<long>();
        }
    }
}
