namespace BOTS.Data.Models
{
    public class Currency
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public virtual ICollection<CurrencyPair> LeftPairs { get; set; }
            = new HashSet<CurrencyPair>();

        public virtual ICollection<CurrencyPair> RightPairs { get; set; }
            = new HashSet<CurrencyPair>();
    }
}
