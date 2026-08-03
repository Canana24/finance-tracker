using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
using FinanceTracker.API.Services;
using Moq;

namespace FinanceTracker.API.Tests.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _reportRepository = new();
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _service = new ReportService(_reportRepository.Object);
        }

        private static Currency Uyu => new() { Id = 1, Code = "UYU", Name = "Peso uruguayo", Symbol = "$" };
        private static Currency Usd => new() { Id = 2, Code = "USD", Name = "Dólar", Symbol = "US$" };

        private static Transaction MakeTransaction(string type, decimal amount, DateTime date, Currency currency, Category? category = null)
        {
            category ??= new Category { Id = 1, Name = "Otros", Type = type };
            return new Transaction
            {
                Id = 1,
                Type = type,
                Amount = amount,
                Date = date,
                Currency = currency,
                CurrencyId = currency.Id,
                Category = category,
                CategoryId = category.Id,
                IsActive = true
            };
        }

        private static ExchangeRate MakeRate(decimal rate, DateTime date, Currency currency) => new()
        {
            Currency = currency,
            CurrencyId = currency.Id,
            Rate = rate,
            Date = date,
            BaseCurrency = Uyu,
            BaseCurrencyId = Uyu.Id
        };

        // --- ConvertToUYU (probado indirectamente vía GetMonthlySummary) ---

        [Fact]
        public async Task GetMonthlySummary_UYUTransaction_NoConversionApplied()
        {
            var transaction = MakeTransaction("INCOME", 1000m, new DateTime(2026, 3, 15), Uyu);
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(new[] { transaction });
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(new[] { MakeRate(40m, new DateTime(2026, 3, 1), Usd) });

            var result = await _service.GetMonthlySummary(1, 3, 2026);

            Assert.Equal(1000m, result.TotalIncome);
        }

        [Fact]
        public async Task GetMonthlySummary_UsesExactDateRate_WhenAvailable()
        {
            var txDate = new DateTime(2026, 3, 15);
            var transaction = MakeTransaction("EXPENSE", 100m, txDate, Usd);

            var rates = new[]
            {
                MakeRate(38m, new DateTime(2026, 3, 10), Usd),
                MakeRate(40m, txDate, Usd), // match exacto
                MakeRate(42m, new DateTime(2026, 3, 20), Usd), // posterior, no debe usarse
            };
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(new[] { transaction });
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(rates);

            var result = await _service.GetMonthlySummary(1, 3, 2026);

            Assert.Equal(4000m, result.TotalExpenses); // 100 * 40
        }

        [Fact]
        public async Task GetMonthlySummary_UsesNearestPriorRate_WhenNoExactMatch()
        {
            var txDate = new DateTime(2026, 3, 15);
            var transaction = MakeTransaction("EXPENSE", 100m, txDate, Usd);

            var rates = new[]
            {
                MakeRate(35m, new DateTime(2026, 3, 1), Usd),
                MakeRate(38m, new DateTime(2026, 3, 10), Usd), // más cercana anterior
                MakeRate(45m, new DateTime(2026, 3, 20), Usd), // posterior, no debe usarse
            };
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(new[] { transaction });
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(rates);

            var result = await _service.GetMonthlySummary(1, 3, 2026);

            Assert.Equal(3800m, result.TotalExpenses); // 100 * 38
        }

        [Fact]
        public async Task GetMonthlySummary_FallsBackToOldestRate_WhenAllRatesAreAfterTransactionDate()
        {
            var txDate = new DateTime(2026, 1, 5);
            var transaction = MakeTransaction("EXPENSE", 100m, txDate, Usd);

            var rates = new[]
            {
                MakeRate(45m, new DateTime(2026, 3, 20), Usd), // la más antigua disponible, pero posterior a la transacción
                MakeRate(50m, new DateTime(2026, 4, 1), Usd),
            };
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 1, 2026)).ReturnsAsync(new[] { transaction });
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(rates);

            var result = await _service.GetMonthlySummary(1, 1, 2026);

            Assert.Equal(4500m, result.TotalExpenses); // 100 * 45 (la más antigua)
        }

        [Fact]
        public async Task GetMonthlySummary_NoRateForCurrency_ReturnsAmountUnconverted()
        {
            var transaction = MakeTransaction("EXPENSE", 100m, new DateTime(2026, 3, 15), Usd);

            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(new[] { transaction });
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = await _service.GetMonthlySummary(1, 3, 2026);

            Assert.Equal(100m, result.TotalExpenses);
        }

        [Fact]
        public async Task GetMonthlySummary_ComputesSavingsAndTransactionCount()
        {
            var transactions = new[]
            {
                MakeTransaction("INCOME", 1000m, new DateTime(2026, 3, 5), Uyu),
                MakeTransaction("EXPENSE", 400m, new DateTime(2026, 3, 6), Uyu),
            };
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(transactions);
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = await _service.GetMonthlySummary(1, 3, 2026);

            Assert.Equal(1000m, result.TotalIncome);
            Assert.Equal(400m, result.TotalExpenses);
            Assert.Equal(600m, result.Savings);
            Assert.Equal(2, result.TransactionCount);
        }

        // --- GetExpensesByCategory ---

        [Fact]
        public async Task GetExpensesByCategory_ComputesPercentageOverTotal()
        {
            var foodCategory = new Category { Id = 1, Name = "Comida", Type = "EXPENSE" };
            var transportCategory = new Category { Id = 2, Name = "Transporte", Type = "EXPENSE" };

            var transactions = new[]
            {
                MakeTransaction("EXPENSE", 300m, new DateTime(2026, 3, 5), Uyu, foodCategory),
                MakeTransaction("EXPENSE", 100m, new DateTime(2026, 3, 6), Uyu, transportCategory),
            };
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(transactions);
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = (await _service.GetExpensesByCategory(1, 3, 2026)).ToList();

            var food = result.Single(c => c.CategoryId == 1);
            var transport = result.Single(c => c.CategoryId == 2);
            Assert.Equal(75m, food.Percentage);
            Assert.Equal(25m, transport.Percentage);
        }

        [Fact]
        public async Task GetExpensesByCategory_NoExpenses_DoesNotDivideByZero()
        {
            _reportRepository.Setup(r => r.GetTransactionsByMonth(1, 3, 2026)).ReturnsAsync(Array.Empty<Transaction>());
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = await _service.GetExpensesByCategory(1, 3, 2026);

            Assert.Empty(result);
        }

        // --- GetMonthlyEvolution ---

        [Fact]
        public async Task GetMonthlyEvolution_AlwaysReturnsTwelveMonths()
        {
            _reportRepository.Setup(r => r.GetTransactionsByYear(1, 2026)).ReturnsAsync(Array.Empty<Transaction>());
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = (await _service.GetMonthlyEvolution(1, 2026)).ToList();

            Assert.Equal(12, result.Count);
            Assert.Equal(Enumerable.Range(1, 12), result.Select(m => m.Month));
            Assert.All(result, m => Assert.Equal(0m, m.Income));
        }

        [Fact]
        public async Task GetMonthlyEvolution_PlacesTransactionsInCorrectMonth()
        {
            var transactions = new[]
            {
                MakeTransaction("INCOME", 500m, new DateTime(2026, 6, 10), Uyu),
                MakeTransaction("EXPENSE", 200m, new DateTime(2026, 6, 20), Uyu),
            };
            _reportRepository.Setup(r => r.GetTransactionsByYear(1, 2026)).ReturnsAsync(transactions);
            _reportRepository.Setup(r => r.GetRatesWithBaseUYU()).ReturnsAsync(Array.Empty<ExchangeRate>());

            var result = (await _service.GetMonthlyEvolution(1, 2026)).ToList();

            var june = result.Single(m => m.Month == 6);
            Assert.Equal(500m, june.Income);
            Assert.Equal(200m, june.Expense);
            Assert.Equal(300m, june.Savings);

            var otherMonths = result.Where(m => m.Month != 6);
            Assert.All(otherMonths, m => Assert.Equal(0m, m.Income + m.Expense));
        }
    }
}
