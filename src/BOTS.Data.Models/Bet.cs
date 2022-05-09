namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Bet
    {
        public Bet()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public BetType Type { get; set; }

        public string UserId { get; set; } = default!;

        public virtual ApplicationUser User { get; set; } = default!;

        [Column(TypeName = "money")]
        public decimal Payout { get; set; }

        [Column(TypeName = "money")]
        public decimal EntryFee { get; set; }

        public byte BarrierIndex { get; set; }

        public string TradingWindowId { get; set; } = default!;

        public virtual TradingWindow TradingWindow { get; set; } = default!;
    }
}
