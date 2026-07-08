using FinanceTracker.API.DTOs.Transaction;

namespace FinanceTracker.API.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponseDto>> GetAllTransactionsByUserId(int userId);
        Task<TransactionResponseDto?> GetTransactionById(int id, int userId);
        Task<TransactionResponseDto> CreateTransaction(CreateTransactionDto dto, int userId);
        Task<TransactionResponseDto?> UpdateTransaction(int id, UpdateTransactionDto dto, int userId);
        Task<bool> DeleteTransaction(int id, int userId);
    }
}
