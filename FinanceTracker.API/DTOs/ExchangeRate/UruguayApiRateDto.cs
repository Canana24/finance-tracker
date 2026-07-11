using System.Text.Json.Serialization;

namespace FinanceTracker.API.DTOs.ExchangeRate
{
    public class UruguayApiRateDto
    {
        [JsonPropertyName("bid")]
        public string Bid { get; set; } = string.Empty;
        [JsonPropertyName("ask")]
        public string Ask { get; set; } = string.Empty;

    }
}
