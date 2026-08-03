using System.Net;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Account;
using FinanceTracker.API.DTOs.Category;
using FinanceTracker.API.DTOs.Report;
using FinanceTracker.API.DTOs.Transaction;

namespace FinanceTracker.API.Tests.Integration
{
    public class ReportControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly ApiTestFactory _factory;

        public ReportControllerTests(ApiTestFactory factory)
        {
            _factory = factory;
        }

        private record CurrencyDto(int Id, string Code, string Name, string Symbol);

        [Fact]
        public async Task GetMonthlyEvolution_ReturnsTwelveMonths()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);

            var response = await client.GetAsync("/api/report/monthly-evolution?year=2026");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var evolution = await response.Content.ReadFromJsonAsync<List<MonthlyEvolutionDto>>();
            Assert.Equal(12, evolution!.Count);
        }

        [Fact]
        public async Task GetMonthlySummary_ReflectsCreatedTransactions()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);

            var currencyResponse = await client.GetAsync("/api/currency");
            var currencies = await currencyResponse.Content.ReadFromJsonAsync<List<CurrencyDto>>();
            var currencyId = currencies!.Single(c => c.Code == "UYU").Id;

            var accountResponse = await client.PostAsJsonAsync("/api/account",
                new CreateAccountDto { Name = "Cuenta", CurrencyId = currencyId, InitialBalance = 0m });
            var account = await accountResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

            var categoryResponse = await client.PostAsJsonAsync("/api/category",
                new CreateCategoryDto { Name = "Sueldo", Type = "INCOME" });
            var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();

            var fixedDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc);
            await client.PostAsJsonAsync("/api/transaction", new CreateTransactionDto
            {
                AccountId = account!.Id,
                CategoryId = category!.Id,
                Amount = 1000m,
                Type = "INCOME",
                Date = fixedDate,
            });

            var response = await client.GetAsync("/api/report/monthly-summary?month=5&year=2026");
            var summary = await response.Content.ReadFromJsonAsync<MonthlySummaryDto>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1000m, summary!.TotalIncome);
            Assert.Equal(1, summary.TransactionCount);
        }

        [Fact]
        public async Task GetExpensesByCategory_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/report/expenses-by-category?month=1&year=2026");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
