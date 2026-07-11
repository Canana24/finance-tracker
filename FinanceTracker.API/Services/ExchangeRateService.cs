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

        public async Task <IEnumerable<ExchangeRateResponseDto>> RefreshRatesFromFrankfurter (string baseCurrency)
        {
            //Request
            var url = $"https://api.frankfurter.dev/v1/latest?base={baseCurrency}";
            var response = await GetWithRetry(url);
            response.EnsureSuccessStatusCode();

            //Leer y deserializar JSON
            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; //Para ignorar mayusculas y minusculas
            var data = JsonSerializer.Deserialize<FrankfurterResponseDto>(json, options);

            if(data == null)
            {
                throw new Exception("Error al deserializar la respuesta de la API de Frankfurter.");
            }

            //Mapear
            var baseCurrencyEntity = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == data.Base);
            
            if (baseCurrencyEntity == null)
            {
                throw new Exception($"La moneda base '{data.Base}' no se encuentra en la base de datos.");
            }

            var rateDate = DateTime.Parse(data.Date, CultureInfo.InvariantCulture);
            var existingRates = await _context.ExchangeRates
                .Where(e => e.Date == rateDate && e.BaseCurrencyId == baseCurrencyEntity.Id)
                .ToArrayAsync();

            if (existingRates.Any()) { _context.ExchangeRates.RemoveRange(existingRates); } //Para no cargar dos veces el mismo dia

            var result = new List<ExchangeRateResponseDto>();

            foreach (var rate in data.Rates) { 
                var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == rate.Key);

                if (currency == null) { continue; }

                var exchangeRate = new ExchangeRate
                {
                    CurrencyId = currency.Id,
                    BaseCurrencyId = baseCurrencyEntity.Id,
                    Rate = rate.Value,
                    Date = rateDate
                };
                _context.ExchangeRates.Add(exchangeRate);

                //Preparar el DTO de respuesta
                result.Add(new ExchangeRateResponseDto
                {
                    CurrencyCode = currency.Code,
                    BaseCurrencyCode = data.Base,
                    Rate = rate.Value,
                    Date = rateDate
                });
            }

            await _context.SaveChangesAsync();
            return result;
        }
        
        public async Task<IEnumerable<ExchangeRateResponseDto>> GetLatestRates()
        {
            var latestDate = await _context.ExchangeRates
                .MaxAsync(e => (DateTime?)e.Date);

            if (latestDate == null) { return new List<ExchangeRateResponseDto>(); }
            
            return await _context.ExchangeRates
                .Where(e => e.Date == latestDate)
                .Include(e => e.Currency)
                .Include(e => e.BaseCurrency)
                .Select(e => new ExchangeRateResponseDto
                {
                    CurrencyCode = e.Currency.Code,
                    BaseCurrencyCode = e.BaseCurrency!.Code,
                    Rate = e.Rate,
                    Date = e.Date
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ExchangeRateResponseDto>> RefreshRatesFromUruguayApi()
        {
            var url = "https://uruguayapi.onrender.com/api/v1/banks/brou_rates";
            var response = await GetWithRetry(url);

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var data = JsonSerializer.Deserialize<Dictionary<string, UruguayApiRateDto>>(json, options);

            if (data == null)
                throw new Exception("No se pudo interpretar la respuesta de UruguayAPI.");

            //Para poder usarlo con codigo ISO
            var nameToISO = new Dictionary<string, string>
            {
                { "dolar", "USD" },
                { "euro", "EUR" },
                { "real", "BRL" },
                { "peso_argentino", "ARS" }
            };

            var baseCurrencyEntity = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == "UYU");
            var rateDate = DateTime.Now.Date;

            var existingRates = await _context.ExchangeRates
                .Where(e => e.Date == rateDate && e.BaseCurrencyId == baseCurrencyEntity!.Id)
                .ToArrayAsync();

            if(existingRates.Any()) { _context.ExchangeRates.RemoveRange(existingRates); } //Para no cargar dos veces el mismo dia

            var result = new List<ExchangeRateResponseDto>();

            foreach(var item in data)
            {
                if (!nameToISO.ContainsKey(item.Key)) {  continue; }

                var isoCode = nameToISO[item.Key];
                var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == isoCode);

                if(currency == null) { continue; }

                var bid = decimal.Parse(item.Value.Bid, new CultureInfo("es-UY"));
                var ask = decimal.Parse(item.Value.Ask, new CultureInfo("es-UY"));

                var averageRate = (bid + ask) / 2;

                var exchangeRate = new ExchangeRate
                {
                    CurrencyId = currency.Id,
                    BaseCurrencyId = baseCurrencyEntity!.Id,
                    Rate = averageRate,
                    Date = rateDate
                };
                _context.ExchangeRates.Add(exchangeRate);

                result.Add(new ExchangeRateResponseDto
                {
                    CurrencyCode = currency.Code,
                    BaseCurrencyCode = baseCurrencyEntity!.Code,
                    Rate = averageRate,
                    Date = rateDate
                });
            }
            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<HttpResponseMessage> GetWithRetry(string url, int maxRetries = 3)
        { 
            var client = _httpClientFactory.CreateClient();
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return response;

                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(2000);
                    }
                }
                catch(TaskCanceledException ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(2000);
                    }
                }
            }
            throw new Exception(
            $"No se pudo conectar con el servicio de cotizaciones después de {maxRetries} intentos. " +
            $"Detalle: {lastException?.Message}");
        }

    }
}
