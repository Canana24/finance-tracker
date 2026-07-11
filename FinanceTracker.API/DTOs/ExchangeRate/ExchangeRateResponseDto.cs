namespace FinanceTracker.API.DTOs.ExchangeRate
{
    public class ExchangeRateResponseDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string BaseCurrencyCode { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
