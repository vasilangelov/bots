namespace BOTS.Data.Models
{
    public class TradingWindow
    {
        public Guid Id { get; set; }

        public bool IsClosed { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public TimeSpan Duration { get; set; }

        public virtual ICollection<BettingOption> BettingOptions { get; set; }
            = new HashSet<BettingOption>();
    }
}
