namespace BOTS.Web.Models.ViewModels
{
    using AutoMapper;

    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class BetHistoryViewModel : ICustomMap
    {
        private const string CurrencyPairFormat = "{0}/{1}";

        public string Id { get; set; } = default!;

        public string CurrencyPair { get; set; } = default!;

        public string Type { get; set; } = default!;

        public DateTime EndsOn { get; set; } = default!;

        public decimal BarrierPrediction { get; set; }

        public decimal Payout { get; set; }

        public decimal EntryFee { get; set; }

        public bool IsWinningBet { get; set; }

        public void ConfigureMap(IProfileExpression configuration)
        {
            configuration.CreateMap<Bet, BetHistoryViewModel>()
                .ForMember(x => x.CurrencyPair, x => x.MapFrom(opt => string.Format(
                    CurrencyPairFormat,
                    opt.BettingOption.CurrencyPair.CurrencyFrom.Name,
                    opt.BettingOption.CurrencyPair.CurrencyTo.Name)))
                .ForMember(
                    x => x.EndsOn,
                    opt => opt.MapFrom(y => DateTime.SpecifyKind(y.BettingOption.TradingWindow.End, DateTimeKind.Utc)))
                .ForMember(
                    x => x.IsWinningBet,
                    opt => opt.MapFrom(y => (y.Type == BetType.Higher && y.BarrierPrediction < y.BettingOption.CloseValue) ||
                                            (y.Type == BetType.Lower && y.BarrierPrediction > y.BettingOption.CloseValue)));
        }
    }
}
