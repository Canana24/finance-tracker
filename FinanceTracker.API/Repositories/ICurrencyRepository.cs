using FinanceTracker.API.Models;
namespace FinanceTracker.API.Repositories
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int id);
    }
}
