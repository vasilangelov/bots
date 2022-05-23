namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Services.CurrencyRateStats.Models;
    using BOTS.Services.Mapping;

    public class CurrencyRateHistoryViewModel : ICustomMap
    {
        public string Time { get; set; } = default!;

        public decimal Open { get; set; }

        public decimal High { get; set; }

        public decimal Low { get; set; }

        public decimal Close { get; set; }

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<CurrencyRateStat, CurrencyRateHistoryViewModel>()
                .ForMember(
                x => x.Time,
                x => x.MapFrom(y => DateTime.SpecifyKind(y.Time, DateTimeKind.Utc).ToString("O")));
        }
    }
}
