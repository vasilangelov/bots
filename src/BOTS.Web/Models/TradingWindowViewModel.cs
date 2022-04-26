namespace BOTS.Web.Models
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class TradingWindowViewModel : IMapFrom<TradingWindow>
    {
        public string Id { get; set; } = default!;

        public decimal OpeningPrice { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int OptionBarrierCount { get; set; }

        public decimal OptionBarrierStep { get; set; }
    }
}
