namespace BOTS.Services.Models
{
    using System.Text.Json.Serialization;

    public class CurrencyInfo
    {
        public string Base { get; set; } = default!;

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public IDictionary<string, decimal> Rates { get; set; } = default!;
    }
}
