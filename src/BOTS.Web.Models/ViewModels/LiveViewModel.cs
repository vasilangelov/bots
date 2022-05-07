namespace BOTS.Web.Models.ViewModels
{
    public class LiveViewModel
    {
        public IEnumerable<CurrencyPairViewModel> CurrencyPairs { get; set; } = default!;
    }
}
