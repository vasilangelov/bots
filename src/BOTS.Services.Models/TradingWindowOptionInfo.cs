namespace BOTS.Services.Models
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class TradingWindowOptionInfo : IMapFrom<TradingWindowSetting>
    {
        public int Id { get; set; }

        public int BarrierCount { get; set; }

        public decimal BarrierStep { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
