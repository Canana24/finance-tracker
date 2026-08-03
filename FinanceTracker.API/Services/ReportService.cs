using FinanceTracker.API.Models;
using FinanceTracker.API.DTOs.Report;
using FinanceTracker.API.Repositories;

namespace FinanceTracker.API.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<MonthlySummaryDto> GetMonthlySummary(int userId, int month, int year)
        {
            var transactions = await _reportRepository.GetTransactionsByMonth(userId, month, year);
            var rates = await _reportRepository.GetRatesWithBaseUYU();

            decimal totalIncome = 0;
            decimal totalExpense = 0;

            foreach (var transaction in transactions) {
                var amountInUYU = ConvertToUYU(transaction, rates);

                if (transaction.Type == "INCOME") { 
                    totalIncome += amountInUYU;
                }
                else
                {
                    totalExpense += amountInUYU;
                }
            }

            return new MonthlySummaryDto
            {
                Month = month,
                Year = year,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Savings = totalIncome - totalExpense,
                TransactionCount = transactions.Count()
            };
        }

        public async Task<IEnumerable<CategoryExpenseDto>> GetExpensesByCategory (int userId, int month, int year)
        {
            var transactions = await _reportRepository.GetTransactionsByMonth(userId, month, year);
            var rates = await _reportRepository.GetRatesWithBaseUYU();

            var expenses = transactions.Where(t => t.Type == "EXPENSE");

            var groupedExpenses = expenses
                .GroupBy(t => new {t.CategoryId, CategoryName = t.Category.Name})
                .Select( g => new CategoryExpenseDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Total = g.Sum(t => ConvertToUYU(t, rates))
                })
                .ToList();

            var totalExpenses = groupedExpenses.Sum(e => e.Total);

            foreach (var category in groupedExpenses) 
            {
                category.Percentage = totalExpenses == 0 ? 0 : Math.Round(category.Total / totalExpenses * 100, 2);
            }

            return groupedExpenses.OrderByDescending(c => c.Total);
        }

        public async Task<IEnumerable<MonthlyEvolutionDto>> GetMonthlyEvolution(int userId, int year)
        {
            var transactions = await _reportRepository.GetTransactionsByYear(userId, year);
            var rates = await _reportRepository.GetRatesWithBaseUYU();

            var result = new List<MonthlyEvolutionDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = transactions.Where(t => t.Date.Month == month);
                decimal income = 0;
                decimal expense = 0;

                foreach (var transaction in monthTransactions)
                {
                    var amountInUYU = ConvertToUYU(transaction, rates);

                    if (transaction.Type == "INCOME")
                    {
                        income += amountInUYU;
                    }
                    else
                    {
                        expense += amountInUYU;
                    }
                }
                result.Add(new MonthlyEvolutionDto
                {
                    Month = month,
                    Income = income,
                    Expense = expense,
                    Savings = income - expense,
                });
            }

            return result;
        }

        //Convierte transaccion a peso uruguayo
        private static decimal ConvertToUYU(Transaction transaction, IEnumerable<ExchangeRate> rates)
        {
            var currencyCode = transaction.Currency?.Code ?? string.Empty;

            if (currencyCode == "UYU")
                return transaction.Amount;

            // Busca la cotización cercana ANTERIOR o igual 
            var rate = rates
                .Where(r => r.Currency.Code == currencyCode && r.Date <= transaction.Date)
                .OrderByDescending(r => r.Date)
                .FirstOrDefault();

            if (rate == null)
            {
                rate = rates
                    .Where(r => r.Currency.Code == currencyCode)
                    .OrderBy(r => r.Date)
                    .FirstOrDefault();
            }

            if (rate == null) 
            {
                return transaction.Amount;
            }

            return transaction.Amount * rate.Rate;
        }
    }
}
