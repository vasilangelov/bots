namespace BOTS.Web.Models
{
    public class LiveViewModel
    {
        public IEnumerable<CurrencyPairViewModel> CurrencyPairs { get; set; } = default!;
    }
}
