namespace BOTS.Services.Models
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class TradingWindowOptionInfo : IMapFrom<TradingWindowOption>
    {
        public int Id { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
