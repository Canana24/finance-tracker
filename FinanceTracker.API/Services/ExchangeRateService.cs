using FinanceTracker.API.Data;
using FinanceTracker.API.DTOs.ExchangeRate;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace FinanceTracker.API.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly FinanceTrackerContext _context;

        public ExchangeRateService(IHttpClientFactory httpClientFactory, FinanceTrackerContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task <IEnumerable<ExchangeRate>> RefreshRatesFromFrankfurter (string baseCurrency)
        {
            // Crear el cliente HTTP
            var client = _httpClientFactory.CreateClient();

            //Armar la URL
            var url = $"https://api.frankfurter.dev/v1/latest?base={baseCurrency}";

            //Request
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode(); //Excepcion por si falla

            //Leer y deserializar JSON
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //Para ignorar mayusculas y minusculas
            var data = JsonSerializer.Deserialize<FrankfurterResponseDto>(json, options);

            if(data == null)
            {
                throw new Exception("Error al deserializar la respuesta de la API de Frankfurter.");
            }

            //Mapear
            var savedRates = new List<ExchangeRate>();
            var rateDate = DateTime.Parse(data.Date, CultureInfo.InvariantCulture);

            foreach (var rate in data.Rates) { 
                var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rate.Key);

                if (currency == null) { continue; }

                var exchangeRate = new ExchangeRate
                {
                    CurrencyId = currency.Id,
                    Rate = rate.Value,
                    Date = rateDate
                };
                _context.ExchangeRates.Add(exchangeRate);
                savedRates.Add(exchangeRate);
            }

            await _context.SaveChangesAsync();
            return savedRates;
        }
        
        public async Task<IEnumerable<ExchangeRate>> GetLatestRates()
        {
            var latestDate = await _context.ExchangeRates
                .MaxAsync(e => (DateTime?)e.Date);

            if (latestDate == null) { return new List<ExchangeRate>(); }
            
            return await _context.ExchangeRates
                .Where(e => e.Date == latestDate)
                .Include(e => e.Currency)
                .ToListAsync();
        }
    
    }
}
