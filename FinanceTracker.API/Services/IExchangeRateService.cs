using FinanceTracker.API.DTOs.ExchangeRate;
using FinanceTracker.API.Models;

namespace FinanceTracker.API.Services
{
    public interface IExchangeRateService
    {
        Task<IEnumerable<ExchangeRateResponseDto>> RefreshRatesFromFrankfurter(string baseCurrency);
        Task<IEnumerable<ExchangeRateResponseDto>> GetLatestRates();
        Task<IEnumerable<ExchangeRateResponseDto>> RefreshRatesFromUruguayApi();
    }
}
