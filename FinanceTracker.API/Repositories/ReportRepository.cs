using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;
namespace FinanceTracker.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly FinanceTrackerContext _context;

        public ReportRepository(FinanceTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByMonth(int userId, int month, int year)
        {
            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1);

            return await _context.Transactions
                .Where(t => t.Account.UserId == userId && (t.Date >= from && t.Date < to) && t.Date.Year == year && t.IsActive == true)
                .Include(t => t.Category)
                .Include(t => t.Currency)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByYear(int userId, int year)
        {
            var from = new DateTime(year, 1, 1);
            var to = from.AddYears(1);
            return await _context.Transactions
                .Where(t => t.Account.UserId == userId && (t.Date >= from && t.Date < to) && t.Date.Year == year && t.IsActive == true)
                .Include(t => t.Category)
                .Include(t => t.Currency)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExchangeRate>> GetRatesWithBaseUYU()
        {
            return await _context.ExchangeRates
                .Include(r => r.BaseCurrency)
                .Include(r => r.Currency)
                .Where(r => r.BaseCurrency!.Code == "UYU")
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }
    }
}
