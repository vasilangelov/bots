namespace BOTS.Web.Models
{
    using BOTS.Data.Models;
    using BOTS.Services.Mapping;

    public class CurrencyPairViewModel : IMapFrom<CurrencyPair>
    {
        public int Id { get; set; }

        public string LeftName { get; set; } = default!;

        public string RightName { get; set; } = default!;
    }
}
