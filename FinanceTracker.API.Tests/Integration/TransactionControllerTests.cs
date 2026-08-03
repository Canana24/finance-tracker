using System.Net;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Account;
using FinanceTracker.API.DTOs.Category;
using FinanceTracker.API.DTOs.Transaction;

namespace FinanceTracker.API.Tests.Integration
{
    public class TransactionControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly ApiTestFactory _factory;

        public TransactionControllerTests(ApiTestFactory factory)
        {
            _factory = factory;
        }

        private record CurrencyDto(int Id, string Code, string Name, string Symbol);

        private async Task<(HttpClient client, int accountId, int categoryId)> SetupUserWithAccountAndCategoryAsync()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);

            var currencyResponse = await client.GetAsync("/api/currency");
            var currencies = await currencyResponse.Content.ReadFromJsonAsync<List<CurrencyDto>>();
            var currencyId = currencies!.Single(c => c.Code == "UYU").Id;

            var accountResponse = await client.PostAsJsonAsync("/api/account",
                new CreateAccountDto { Name = "Cuenta", CurrencyId = currencyId, InitialBalance = 1000m });
            var account = await accountResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

            var categoryResponse = await client.PostAsJsonAsync("/api/category",
                new CreateCategoryDto { Name = "Sueldo", Type = "INCOME" });
            var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();

            return (client, account!.Id, category!.Id);
        }

        [Fact]
        public async Task CreateTransaction_Income_Returns201AndUpdatesAccountBalance()
        {
            var (client, accountId, categoryId) = await SetupUserWithAccountAndCategoryAsync();

            var dto = new CreateTransactionDto
            {
                AccountId = accountId,
                CategoryId = categoryId,
                Amount = 300m,
                Type = "INCOME",
                Date = DateTime.UtcNow,
            };

            var response = await client.PostAsJsonAsync("/api/transaction", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var accountResponse = await client.GetAsync($"/api/account/{accountId}");
            var account = await accountResponse.Content.ReadFromJsonAsync<AccountResponseDto>();
            Assert.Equal(1300m, account!.Balance);
        }

        [Fact]
        public async Task DeleteTransaction_RevertsAccountBalance()
        {
            var (client, accountId, categoryId) = await SetupUserWithAccountAndCategoryAsync();

            var createResponse = await client.PostAsJsonAsync("/api/transaction", new CreateTransactionDto
            {
                AccountId = accountId,
                CategoryId = categoryId,
                Amount = 300m,
                Type = "INCOME",
                Date = DateTime.UtcNow,
            });
            var created = await createResponse.Content.ReadFromJsonAsync<TransactionResponseDto>();

            var deleteResponse = await client.DeleteAsync($"/api/transaction/{created!.Id}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var accountResponse = await client.GetAsync($"/api/account/{accountId}");
            var account = await accountResponse.Content.ReadFromJsonAsync<AccountResponseDto>();
            Assert.Equal(1000m, account!.Balance); // vuelve al balance original
        }

        [Fact]
        public async Task CreateTransaction_AccountFromAnotherUser_Returns400()
        {
            // El Service lanza ArgumentException si la cuenta no existe o no es del usuario,
            // mapeada por ExceptionMiddleware a 400. (Antes caía a 500; ver historial.)
            var (_, accountId, _) = await SetupUserWithAccountAndCategoryAsync();
            var (otherClient, _, otherCategoryId) = await SetupUserWithAccountAndCategoryAsync();

            var dto = new CreateTransactionDto
            {
                AccountId = accountId, // pertenece al primer usuario
                CategoryId = otherCategoryId,
                Amount = 100m,
                Type = "INCOME",
                Date = DateTime.UtcNow,
            };

            var response = await otherClient.PostAsJsonAsync("/api/transaction", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
