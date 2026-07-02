namespace FinanceTracker.API.DTOs.Account
{
    public class CreateAccountDto
    {
        public string Name { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public decimal InitialBalance { get; set; }
    }

    public class UpdateAccountDto
    {
        public string Name { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
    }

    public class AccountResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
