using FinanceTracker.API.Models;

namespace FinanceTracker.API.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsByUserId(int userId);
        Task<Transaction?> GetTransactionById(int id, int userId);
        Task<Transaction> CreateTransaction(Transaction transaction);
        Task UpdateTransaction(Transaction transaction);
    }
}
