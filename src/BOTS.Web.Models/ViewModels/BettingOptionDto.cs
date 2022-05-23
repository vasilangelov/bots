namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class BettingOptionDto : ICustomMap
    {
        public Guid Id { get; set; } = default!;

        public int CurrencyPairId { get; set; }

        public decimal[] Barriers { get; set; } = default!;

        public decimal BarrierStep { get; set; }

        public DateTime End { get; set; }

        public long Duration { get; set; }

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<BettingOption, BettingOptionDto>()
                .ForMember(x => x.End, x => x.MapFrom(y => y.TradingWindow.End))
                .ForMember(x => x.Duration, x => x.MapFrom(y => (long)y.TradingWindow.Duration.TotalSeconds));
        }
    }
}
