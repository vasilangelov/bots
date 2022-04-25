namespace BOTS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class TradingWindowOption
    {
        public int Id { get; set; }

        public TimeSpan Duration { get; set; }

        [Column(TypeName = "money")]
        public decimal BarrierStep { get; set; }

        public int BarrierCount { get; set; }

        public virtual ICollection<TradingWindow> TradingWindows { get; set; }
            = new HashSet<TradingWindow>();
    }
}
