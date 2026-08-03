using FinanceTracker.API.Models;

namespace FinanceTracker.API.Repositories
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAccountsByUserId(int userId);
        Task<Account?> GetAccountById(int id, int userId);
        Task<Account> CreateAccount(Account account);
        Task UpdateAccount(Account account);
    }
}
