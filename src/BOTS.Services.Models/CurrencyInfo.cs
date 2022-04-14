namespace BOTS.Services.Models
{
    public class CurrencyInfo
    {
        public string Base { get; set; }

        public Dictionary<string, decimal> Rates { get; set; } = default!;
    }
}
