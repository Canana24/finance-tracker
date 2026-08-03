using FinanceTracker.API.Data;
using FinanceTracker.API.DTOs.Auth;
using FinanceTracker.API.Exceptions;
using FinanceTracker.API.Models;
using FinanceTracker.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FinanceTracker.API.Tests.Services
{
    // AuthService habla directo con FinanceTrackerContext (no usa un repository),
    // así que se prueba contra una base InMemory en vez de mockear un repository.
    public class AuthServiceTests
    {
        private readonly FinanceTrackerContext _context;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<FinanceTrackerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new FinanceTrackerContext(options);
            _context.Roles.Add(new Role { Id = 1, Name = "USER" });
            _context.SaveChanges();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:SecretKey"] = "clave-de-prueba-para-tests-unitarios-muy-larga",
                    ["JwtSettings:Issuer"] = "FinanceTrackerTests",
                    ["JwtSettings:Audience"] = "FinanceTrackerTestsUsers",
                    ["JwtSettings:ExpirationMinutes"] = "60",
                })
                .Build();

            _service = new AuthService(_context, configuration);
        }

        private async Task SeedUser(string email, string password, bool isActive = true)
        {
            _context.Users.Add(new User
            {
                Name = "Usuario Existente",
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 1,
                IsActive = isActive
            });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsToken()
        {
            var dto = new RegisterDto { Name = "Juan", Email = "juan@test.com", Password = "Password123" };

            var result = await _service.RegisterAsync(dto);

            Assert.False(string.IsNullOrEmpty(result.Token));
            var stored = await _context.Users.SingleAsync(u => u.Email == "juan@test.com");
            Assert.True(stored.IsActive);
            Assert.NotEqual("Password123", stored.Password); // debe estar hasheada
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123", stored.Password));
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
        {
            await SeedUser("juan@test.com", "Password123");
            var dto = new RegisterDto { Name = "Juan", Email = "juan@test.com", Password = "OtraClave1" };

            await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_EmailDoesNotExist_ThrowsUnauthorizedWithGenericMessage()
        {
            var dto = new LoginDto { Email = "noexiste@test.com", Password = "cualquiera" };

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(dto));
            Assert.Equal("El email o la contraseña no coinciden.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsSameGenericMessageAsEmailNotFound()
        {
            await SeedUser("juan@test.com", "Password123");
            var dto = new LoginDto { Email = "juan@test.com", Password = "ClaveIncorrecta" };

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(dto));

            // El mensaje debe ser idéntico al de "email no existe" para no permitir enumeración de usuarios.
            Assert.Equal("El email o la contraseña no coinciden.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_TreatedAsNotFound()
        {
            await SeedUser("juan@test.com", "Password123", isActive: false);
            var dto = new LoginDto { Email = "juan@test.com", Password = "Password123" };

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(dto));
            Assert.Equal("El email o la contraseña no coinciden.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            await SeedUser("juan@test.com", "Password123");
            var dto = new LoginDto { Email = "juan@test.com", Password = "Password123" };

            var result = await _service.LoginAsync(dto);

            Assert.False(string.IsNullOrEmpty(result.Token));
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }
    }
}
