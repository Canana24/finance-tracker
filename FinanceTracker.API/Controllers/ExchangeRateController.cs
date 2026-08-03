using FinanceTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest() 
        { 
        
            var rates = await _exchangeRateService.GetLatestRates();
            return Ok(rates);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromQuery] string baseCurrency = "USD") 
        { 
        
            var rates = await _exchangeRateService.RefreshRatesFromFrankfurter(baseCurrency);
            return Ok(new
            {
                message = $"Cotizaciones actualizadas desde Frankfurter (base {baseCurrency}).",
                count = rates.Count(),
                rates
            });
        }

        [HttpPost("refresh-uruguay")]
        public async Task<IActionResult> RefreshUruguayApi()
        {
            var rates = await _exchangeRateService.RefreshRatesFromUruguayApi();
            return Ok(new
            {
                message = "Cotizaciones actualizadas desde UruguayAPI (base UYU).",
                count = rates.Count(),
                rates
            });
        }
    }
}
