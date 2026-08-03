using FinanceTracker.API.Models;
namespace FinanceTracker.API.Services
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int id);
    }
}
