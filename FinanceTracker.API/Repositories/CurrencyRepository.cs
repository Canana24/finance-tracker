using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;
namespace FinanceTracker.API.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly FinanceTrackerContext _context;

        public CurrencyRepository(FinanceTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
           return await _context.Currencies.ToListAsync();
        }

        public async Task<Currency?> GetCurrencyByIdAsync(int id)
        {
            return await _context.Currencies.FindAsync(id);
        }
    }
}
