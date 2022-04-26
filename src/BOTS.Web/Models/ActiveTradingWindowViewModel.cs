namespace BOTS.Web.Models
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class ActiveTradingWindowViewModel : ICustomMap
    {
        public string Id { get; set; } = default!;

        public string Start { get; set; } = default!;

        public string End { get; set; } = default!;

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<TradingWindow, ActiveTradingWindowViewModel>()
                .ForMember(x => x.Start,
                           m => m.MapFrom(t => DateTime.SpecifyKind(t.Start, DateTimeKind.Utc).ToString("O")))
                .ForMember(x => x.End,
                           m => m.MapFrom(t => DateTime.SpecifyKind(t.End, DateTimeKind.Utc).ToString("O")));
        }
    }
}
