namespace BOTS.Data.Configuration
{
    using System.Text.Json;

    using BOTS.Common;
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class BettingOptionConfiguration : IEntityTypeConfiguration<BettingOption>
    {
        public void Configure(EntityTypeBuilder<BettingOption> builder)
        {
            builder
                .HasIndex(x => new { x.CurrencyPairId, x.TradingWindowId })
                .IsUnique();

            builder
                .Property(x => x.Barriers)
                .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null),
                                  x => JsonSerializer.Deserialize<decimal[]>(x, (JsonSerializerOptions?)null)!);

            builder
                .Property(x => x.CloseValue)
                .HasPrecision(GlobalConstants.BarrierDigitPrecision, GlobalConstants.DecimalPlacePrecision);
        }
    }
}
