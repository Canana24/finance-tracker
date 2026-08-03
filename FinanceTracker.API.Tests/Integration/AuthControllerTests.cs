using System.Net;
using System.Net.Http.Json;
using FinanceTracker.API.DTOs.Auth;

namespace FinanceTracker.API.Tests.Integration
{
    public class AuthControllerTests : IClassFixture<ApiTestFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(ApiTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_NewUser_Returns200WithToken()
        {
            var dto = new RegisterDto { Name = "Franco", Email = "franco@test.com", Password = "Password123" };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
            Assert.False(string.IsNullOrEmpty(body!.Token));
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            var dto = new RegisterDto { Name = "Franco", Email = "duplicado@test.com", Password = "Password123" };
            await _client.PostAsJsonAsync("/api/auth/register", dto);

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithToken()
        {
            var registerDto = new RegisterDto { Name = "Franco", Email = "login-ok@test.com", Password = "Password123" };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto { Email = "login-ok@test.com", Password = "Password123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            // AuthService.LoginAsync ahora lanza UnauthorizedException, mapeada por
            // ExceptionMiddleware a 401. (Antes caía al branch por defecto -> 500; ver historial.)
            var registerDto = new RegisterDto { Name = "Franco", Email = "login-fail@test.com", Password = "Password123" };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto { Email = "login-fail@test.com", Password = "ClaveIncorrecta" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_EmailDoesNotExist_Returns401WithSameMessageAsWrongPassword()
        {
            var loginDto = new LoginDto { Email = "no-existe@test.com", Password = "cualquiera" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAccounts_WithoutToken_Returns401()
        {
            var response = await _client.GetAsync("/api/account");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
