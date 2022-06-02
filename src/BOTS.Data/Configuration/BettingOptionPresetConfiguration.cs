namespace BOTS.Data.Configuration
{
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class BettingOptionPresetConfiguration : IEntityTypeConfiguration<BettingOptionPreset>
    {
        public void Configure(EntityTypeBuilder<BettingOptionPreset> builder)
        {
            builder
                .HasKey(x => new { x.TradingWindowPresetId, x.CurrencyPairId });
        }
    }
}
