namespace BOTS.Data.Configuration
{
    using BOTS.Common;
    using BOTS.Data.Models;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    internal class BetConfiguration : IEntityTypeConfiguration<Bet>
    {
        public void Configure(EntityTypeBuilder<Bet> builder)
        {
            builder
                .Property(x => x.BarrierPrediction)
                .HasPrecision(GlobalConstants.BarrierDigitPrecision, GlobalConstants.DecimalPlacePrecision);
        }
    }
}
