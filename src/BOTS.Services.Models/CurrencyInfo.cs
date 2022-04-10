namespace BOTS.Services.Models
{
    public class CurrencyInfo
    {
        public Currency Base { get; set; }

        public Dictionary<Currency, decimal> Rates { get; set; } = default!;
    }
}
