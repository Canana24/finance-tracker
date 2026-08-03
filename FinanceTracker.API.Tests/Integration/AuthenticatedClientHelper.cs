using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Auth;

namespace FinanceTracker.API.Tests.Integration
{
    internal static class AuthenticatedClientHelper
    {
        // Registra un usuario nuevo (email único por test) y devuelve un HttpClient
        // con el JWT resultante ya seteado como Bearer, como haría un cliente real.
        public static async Task<HttpClient> CreateAuthenticatedClientAsync(ApiTestFactory factory, string? email = null)
        {
            var client = factory.CreateClient();
            var dto = new RegisterDto
            {
                Name = "Usuario de test",
                Email = email ?? $"user-{Guid.NewGuid()}@test.com",
                Password = "Password123",
            };

            var response = await client.PostAsJsonAsync("/api/auth/register", dto);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenResponseDto>();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.Token);
            return client;
        }
    }
}
