using FinanceTracker.API.Data;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.API.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly FinanceTrackerContext _context;

        public AccountRepository(FinanceTrackerContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Account>> GetAllAccountsByUserId(int userId)
        {
            return await _context.Accounts
                .Include(a => a.Currency)
                .Where(a => a.UserId == userId && a.IsActive == true)
                .ToListAsync();
        }

        public async Task<Account?> GetAccountById(int id, int userId)
        {
            return await _context.Accounts
                .Include(a => a.Currency)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId && a.IsActive == true);
        }

        public async Task<Account> CreateAccount(Account account)
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task UpdateAccount(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
