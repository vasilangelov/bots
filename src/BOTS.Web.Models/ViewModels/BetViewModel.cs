namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class BetViewModel : ICustomMap
    {
        public Guid Id { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string CurrencyPair { get; set; } = default!;

        public decimal BarrierPrediction { get; set; }

        public string EndsOn { get; set; } = default!;

        public decimal Payout { get; set; }

        public decimal EntryFee { get; set; }

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<Bet, BetViewModel>()
                .ForMember(
                    x => x.CurrencyPair,
                    opt => opt.MapFrom(y => y.BettingOption.CurrencyPair.CurrencyFrom.Name + "/" + y.BettingOption.CurrencyPair.CurrencyTo.Name)
                )
                .ForMember(
                    x => x.EndsOn,
                    opt => opt.MapFrom(y => DateTime.SpecifyKind(y.BettingOption.TradingWindow.End, DateTimeKind.Utc).ToString("O")));
        }
    }
}
