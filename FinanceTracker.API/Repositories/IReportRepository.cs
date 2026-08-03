using FinanceTracker.API.Models;
namespace FinanceTracker.API.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsByMonth(int userId, int month, int year);
        Task<IEnumerable<Transaction>> GetTransactionsByYear(int userId, int year);
        Task<IEnumerable<ExchangeRate>> GetRatesWithBaseUYU();
    }
}
