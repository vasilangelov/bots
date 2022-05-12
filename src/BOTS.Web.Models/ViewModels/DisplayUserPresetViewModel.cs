namespace BOTS.Web.Models.ViewModels
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class DisplayUserPresetViewModel : IMapFrom<UserPreset>
    {
        public int CurrencyPairId { get; set; }

        public ChartType ChartType { get; set; }

        public decimal Payout { get; set; }
    }
}
