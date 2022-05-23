namespace BOTS.Data.Models
{
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    using System.ComponentModel.DataAnnotations.Schema;

    public class BettingOption
    {
        public Guid Id { get; set; }

        public int CurrencyPairId { get; set; }

        public virtual CurrencyPair CurrencyPair { get; set; } = default!;

        public Guid TradingWindowId { get; set; }

        public virtual TradingWindow TradingWindow { get; set; } = default!;

        public decimal? CloseValue { get; set; }

        [Column(TypeName = "money")]
        public decimal BarrierStep { get; set; }

        public decimal[] Barriers { get; set; } = default!;

        public virtual ICollection<Bet> Bets { get; set; } = new HashSet<Bet>();
    }
}
