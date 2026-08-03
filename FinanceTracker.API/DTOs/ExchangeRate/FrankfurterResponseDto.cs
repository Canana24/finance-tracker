namespace FinanceTracker.API.DTOs.ExchangeRate
{
    public class FrankfurterResponseDto
    {
        public decimal Amount { get; set; }
        public string Base { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}