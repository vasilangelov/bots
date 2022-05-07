namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class TradingWindowEndViewModel : ICustomMap
    {
        public string End { get; set; } = default!;

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<TradingWindow, TradingWindowEndViewModel>()
                .ForMember(x => x.End,
                           x => x.MapFrom(y => DateTime.SpecifyKind(y.End, DateTimeKind.Utc).ToString("O")));
        }
    }
}
