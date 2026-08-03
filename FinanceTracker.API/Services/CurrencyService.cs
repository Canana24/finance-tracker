using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;

namespace FinanceTracker.API.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }


        public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
        {
            return await _currencyRepository.GetAllCurrenciesAsync();
        }

        public async Task<Currency?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El Id debe ser mayor a cero.");

            return await _currencyRepository.GetCurrencyByIdAsync(id);
        }

        public Task<Currency?> GetCurrencyByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}