namespace BOTS.Services.Currencies.Models
{
    using System.Text.Json.Serialization;

    internal class CurrencyRateInfo
    {
        public string Base { get; set; } = default!;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public IDictionary<string, decimal> Rates { get; set; } = default!;
    }
}
