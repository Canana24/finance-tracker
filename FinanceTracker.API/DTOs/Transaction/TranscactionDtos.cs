namespace FinanceTracker.API.DTOs.Transaction
{
    public class CreateTransactionDto
    {
        public int AccountId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "INCOME" or "EXPENSE"
        public string? Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        
    }

    public class UpdateTransactionDto
    {
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
    }
}
