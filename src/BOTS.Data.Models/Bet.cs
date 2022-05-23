namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class Bet
    {
        public Guid Id { get; set; }

        public BetType Type { get; set; }

        public Guid UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = default!;

        [Column(TypeName = "money")]
        public decimal Payout { get; set; }

        [Column(TypeName = "money")]
        public decimal EntryFee { get; set; }

        public decimal BarrierPrediction { get; set; } = default!;

        public Guid BettingOptionId { get; set; }

        public virtual BettingOption BettingOption { get; set; } = default!;
    }
}
