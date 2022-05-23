namespace BOTS.Data.Models
{
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    using System.ComponentModel.DataAnnotations.Schema;

    public class BettingOptionPreset
    {
        public int TradingWindowPresetId { get; set; }

        public virtual TradingWindowPreset TradingWindowPreset { get; set; } = default!;

        public int CurrencyPairId { get; set; }

        public virtual CurrencyPair CurrencyPair { get; set; } = default!;

        [Column(TypeName = "money")]
        public decimal BarrierStep { get; set; }

        public byte BarrierCount { get; set; }
    }
}
