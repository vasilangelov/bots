namespace BOTS.Data.Models
{
    public class CurrencyPair
    {
        public int Id { get; set; }

        public int CurrencyFromId { get; set; }

        public virtual Currency CurrencyFrom { get; set; } = default!;

        public int CurrencyToId { get; set; }

        public virtual Currency CurrencyTo { get; set; } = default!;

        public bool Display { get; set; }

        public virtual ICollection<BettingOptionPreset> BettingOptionPresets { get; set; }
            = new HashSet<BettingOptionPreset>();
    }
}
