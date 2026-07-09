using FinanceTracker.API.Models;

namespace FinanceTracker.API.Services
{
    public interface IExchangeRateService
    {
        Task<IEnumerable<ExchangeRate>> RefreshRatesFromFrankfurter(string baseCurrency);
        Task<IEnumerable<ExchangeRate>> GetLatestRates();
    }
}
