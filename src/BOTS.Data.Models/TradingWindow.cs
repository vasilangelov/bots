namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class TradingWindow
    {
        public TradingWindow()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public int CurrencyPairId { get; set; }

        public virtual CurrencyPair CurrencyPair { get; set; } = default!;

        public int OptionId { get; set; }

        public virtual TradingWindowOption Option { get; set; } = default!;

        [Column(TypeName = "money")]
        public decimal OpeningPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal? ClosingPrice { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public virtual IEnumerable<Bet> Bets { get; set; } = new HashSet<Bet>();
    }
}
