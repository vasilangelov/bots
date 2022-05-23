namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class BettingOptionViewModel : ICustomMap
    {
        public Guid Id { get; set; } = default!;

        public string Start { get; set; } = default!;

        public string End { get; set; } = default!;

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<BettingOption, BettingOptionViewModel>()
                .ForMember(x => x.Start,
                           m => m.MapFrom(t => DateTime.SpecifyKind(t.TradingWindow.Start, DateTimeKind.Utc).ToString("O")))
                .ForMember(x => x.End,
                           m => m.MapFrom(t => DateTime.SpecifyKind(t.TradingWindow.End, DateTimeKind.Utc).ToString("O")));
        }
    }
}
