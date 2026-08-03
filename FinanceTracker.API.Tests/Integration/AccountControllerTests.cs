using System.Net;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Account;

namespace FinanceTracker.API.Tests.Integration
{
    public class AccountControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly ApiTestFactory _factory;

        public AccountControllerTests(ApiTestFactory factory)
        {
            _factory = factory;
        }

        private static async Task<int> GetUyuCurrencyIdAsync(HttpClient client)
        {
            var response = await client.GetAsync("/api/currency");
            var currencies = await response.Content.ReadFromJsonAsync<List<CurrencyDto>>();
            return currencies!.Single(c => c.Code == "UYU").Id;
        }

        private record CurrencyDto(int Id, string Code, string Name, string Symbol);

        [Fact]
        public async Task CreateAccount_Valid_Returns201WithInitialBalanceAndIsListedAfterwards()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var currencyId = await GetUyuCurrencyIdAsync(client);

            var dto = new CreateAccountDto { Name = "Ahorros", CurrencyId = currencyId, InitialBalance = 1000m };
            var createResponse = await client.PostAsJsonAsync("/api/account", dto);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<AccountResponseDto>();
            Assert.Equal(1000m, created!.Balance);

            var getResponse = await client.GetAsync("/api/account");
            var accounts = await getResponse.Content.ReadFromJsonAsync<List<AccountResponseDto>>();
            Assert.Contains(accounts!, a => a.Name == "Ahorros"); // regresión IsActive
        }

        [Fact]
        public async Task UpdateAccount_DoesNotAcceptBalanceInPayload()
        {
            var client = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var currencyId = await GetUyuCurrencyIdAsync(client);

            var createResponse = await client.PostAsJsonAsync("/api/account",
                new CreateAccountDto { Name = "Cuenta", CurrencyId = currencyId, InitialBalance = 500m });
            var created = await createResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

            var updateResponse = await client.PutAsJsonAsync($"/api/account/{created!.Id}",
                new UpdateAccountDto { Name = "Cuenta Renombrada", CurrencyId = currencyId });
            var updated = await updateResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal(500m, updated!.Balance); // el balance no se movió, solo el nombre
            Assert.Equal("Cuenta Renombrada", updated.Name);
        }

        [Fact]
        public async Task GetAccountById_NotOwnedByUser_Returns404()
        {
            var ownerClient = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var currencyId = await GetUyuCurrencyIdAsync(ownerClient);
            var createResponse = await ownerClient.PostAsJsonAsync("/api/account",
                new CreateAccountDto { Name = "Privada", CurrencyId = currencyId, InitialBalance = 0m });
            var created = await createResponse.Content.ReadFromJsonAsync<AccountResponseDto>();

            var otherUserClient = await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_factory);
            var response = await otherUserClient.GetAsync($"/api/account/{created!.Id}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
