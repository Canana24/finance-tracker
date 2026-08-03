namespace FinanceTracker.API.DTOs.Report
{
    
        //Resumen del mes
        public class MonthlySummaryDto
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public decimal TotalIncome { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal Savings { get; set; }
            public int TransactionCount { get; set; }
        }

        //Gasto por Category
        public class CategoryExpenseDto
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public decimal Percentage { get; set; }
        }

        //Gasto Mensual
        public class MonthlyEvolutionDto
        {
            public int Month { get; set; }
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
            public decimal Savings { get; set; }
    }
    
}
