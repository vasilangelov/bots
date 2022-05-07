namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class BetViewModel : ICustomMap
    {
        public string Id { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string CurrencyPair { get; set; } = default!;

        public decimal Barrier { get; set; }

        public string EndsOn { get; set; } = default!;

        public decimal Payout { get; set; }

        public decimal EntryFee { get; set; }

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<Bet, BetViewModel>()
                .ForMember(
                    x => x.Barrier,
                    opt => opt.MapFrom(y => y.TradingWindow.OpeningPrice + (y.BarrierIndex - y.TradingWindow.Option.BarrierCount / 2) * y.TradingWindow.Option.BarrierStep))
                .ForMember(
                    x => x.CurrencyPair,
                    opt => opt.MapFrom(y => y.TradingWindow.CurrencyPair.Left.Name + "/" + y.TradingWindow.CurrencyPair.Right.Name)
                )
                .ForMember(
                    x => x.EndsOn,
                    opt => opt.MapFrom(y => DateTime.SpecifyKind(y.TradingWindow.End, DateTimeKind.Utc).ToString("O")));
        }
    }
}
