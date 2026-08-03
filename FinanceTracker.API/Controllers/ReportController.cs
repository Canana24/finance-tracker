using FinanceTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace FinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet("monthly-summary")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year)
        {
            var summary = await _reportService.GetMonthlySummary(GetUserId(), month, year);
            return Ok(summary);
        }

        [HttpGet("expenses-by-category")]
        public async Task<IActionResult> GetExpensesByCategory([FromQuery] int month, [FromQuery] int year)
        {
            var expenses = await _reportService.GetExpensesByCategory(GetUserId(), month, year);
            return Ok(expenses);
        }

        [HttpGet("monthly-evolution")]
        public async Task<IActionResult> GetMonthlyEvolution([FromQuery] int year)
        {
            var evolution = await _reportService.GetMonthlyEvolution(GetUserId(), year);
            return Ok(evolution);
        }

    }
}
