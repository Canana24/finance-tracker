using FinanceTracker.API.DTOs.Report;
namespace FinanceTracker.API.Services
{
    public interface IReportService
    {
        Task <MonthlySummaryDto> GetMonthlySummary(int userId, int month, int year);
        Task <IEnumerable<CategoryExpenseDto>> GetExpensesByCategory(int userId, int month, int year);
        Task <IEnumerable<MonthlyEvolutionDto>> GetMonthlyEvolution(int userId, int year);
    }
}
