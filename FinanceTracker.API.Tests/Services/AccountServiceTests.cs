using FinanceTracker.API.DTOs.Account;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
using FinanceTracker.API.Services;
using Moq;

namespace FinanceTracker.API.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepository = new();
        private readonly AccountService _service;

        public AccountServiceTests()
        {
            _service = new AccountService(_accountRepository.Object);
        }

        // Regresión: bug real donde CreateAccount no seteaba IsActive y la cuenta
        // quedaba invisible en los GET (que filtran por IsActive == true).
        [Fact]
        public async Task CreateAccount_SetsIsActiveTrueAndUsesInitialBalanceAsStartingBalance()
        {
            Account? created = null;
            _accountRepository.Setup(r => r.CreateAccount(It.IsAny<Account>()))
                .Callback<Account>(a => { a.Id = 1; created = a; })
                .ReturnsAsync((Account a) => a);
            _accountRepository.Setup(r => r.GetAccountById(1, 10))
                .ReturnsAsync(() => created);

            var dto = new CreateAccountDto { Name = "Ahorros", CurrencyId = 1, InitialBalance = 5000m };

            await _service.CreateAccount(dto, userId: 10);

            Assert.NotNull(created);
            Assert.True(created!.IsActive);
            Assert.Equal(5000m, created.Balance);
        }

        [Fact]
        public async Task UpdateAccount_NotFound_ReturnsNull()
        {
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync((Account?)null);

            var dto = new UpdateAccountDto { Name = "Nueva", CurrencyId = 1 };
            var result = await _service.UpdateAccount(1, dto, userId: 10);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAccount_DoesNotChangeBalance()
        {
            // UpdateAccountDto no tiene campo Balance: el saldo solo lo mueven las transacciones.
            var existing = new Account { Id = 1, UserId = 10, Name = "Vieja", CurrencyId = 1, Balance = 1234.56m, IsActive = true };
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(existing);

            var dto = new UpdateAccountDto { Name = "Nueva", CurrencyId = 2 };
            var result = await _service.UpdateAccount(1, dto, userId: 10);

            Assert.NotNull(result);
            Assert.Equal(1234.56m, result!.Balance);
            Assert.Equal("Nueva", result.Name);
        }

        [Fact]
        public async Task DeleteAccount_NotFound_ReturnsFalse()
        {
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync((Account?)null);

            var result = await _service.DeleteAccount(1, userId: 10);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAccount_Found_SoftDeletesWithoutRemovingRecord()
        {
            var existing = new Account { Id = 1, UserId = 10, Name = "Ahorros", CurrencyId = 1, Balance = 100m, IsActive = true };
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(existing);

            var result = await _service.DeleteAccount(1, userId: 10);

            Assert.True(result);
            Assert.False(existing.IsActive);
            Assert.NotNull(existing.DeletedAt);
            _accountRepository.Verify(r => r.UpdateAccount(existing), Times.Once);
        }
    }
}
