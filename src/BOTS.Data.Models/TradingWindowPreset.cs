namespace BOTS.Data.Models
{
    public class TradingWindowPreset
    {
        public int Id { get; set; }

        public TimeSpan Duration { get; set; }

        public virtual ICollection<BettingOptionPreset> BettingOptionPresets { get; set; }
            = new HashSet<BettingOptionPreset>();
    }
}
