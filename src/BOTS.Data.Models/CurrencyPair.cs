namespace BOTS.Data.Models
{
    public class CurrencyPair
    {
        public int Id { get; set; }

        public int LeftId { get; set; }

        public virtual Currency Left { get; set; } = default!;

        public int RightId { get; set; }

        public virtual Currency Right { get; set; } = default!;

        public bool Display { get; set; }

        public virtual ICollection<TradingWindow> TradingWindows { get; set; }
            = new HashSet<TradingWindow>();
    }
}
