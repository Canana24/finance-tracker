using FinanceTracker.API.Data;
using FinanceTracker.API.DTOs.Transaction;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
using FinanceTracker.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace FinanceTracker.API.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepository = new();
        private readonly Mock<IAccountRepository> _accountRepository = new();
        private readonly FinanceTrackerContext _context;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            // El proveedor InMemory no soporta transacciones reales; se ignora la advertencia
            // porque TransactionService las usa solo para atomicidad, que aquí no se está probando.
            var options = new DbContextOptionsBuilder<FinanceTrackerContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            _context = new FinanceTrackerContext(options);

            _service = new TransactionService(_transactionRepository.Object, _accountRepository.Object, _context);
        }

        private static Account MakeAccount(int id, int userId, decimal balance) => new()
        {
            Id = id,
            UserId = userId,
            CurrencyId = 1,
            Name = "Cuenta",
            Balance = balance,
            IsActive = true
        };

        private static Transaction MakeTransaction(int id, int accountId, string type, decimal amount) => new()
        {
            Id = id,
            AccountId = accountId,
            CategoryId = 1,
            CurrencyId = 1,
            Type = type,
            Amount = amount,
            Date = DateTime.UtcNow,
            IsActive = true
        };

        // --- CreateTransaction: aritmética de balance ---

        [Fact]
        public async Task CreateTransaction_Income_IncreasesAccountBalance()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            Transaction? created = null;
            _transactionRepository.Setup(r => r.CreateTransaction(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => { t.Id = 99; created = t; })
                .ReturnsAsync((Transaction t) => t);
            _transactionRepository.Setup(r => r.GetTransactionById(99, 10))
                .ReturnsAsync(() => created);

            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 500m, Type = "INCOME", Date = DateTime.UtcNow };

            await _service.CreateTransaction(dto, 10);

            Assert.Equal(1500m, account.Balance);
            _accountRepository.Verify(r => r.UpdateAccount(It.Is<Account>(a => a.Balance == 1500m)), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_Expense_DecreasesAccountBalance()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            Transaction? created = null;
            _transactionRepository.Setup(r => r.CreateTransaction(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => { t.Id = 99; created = t; })
                .ReturnsAsync((Transaction t) => t);
            _transactionRepository.Setup(r => r.GetTransactionById(99, 10))
                .ReturnsAsync(() => created);

            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 300m, Type = "EXPENSE", Date = DateTime.UtcNow };

            await _service.CreateTransaction(dto, 10);

            Assert.Equal(700m, account.Balance);
        }

        [Theory]
        [InlineData("TRANSFER")]
        [InlineData("income")]
        [InlineData("")]
        public async Task CreateTransaction_InvalidType_ThrowsArgumentException(string type)
        {
            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 100m, Type = type, Date = DateTime.UtcNow };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(dto, 10));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-50)]
        public async Task CreateTransaction_InvalidAmount_ThrowsArgumentException(decimal amount)
        {
            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = amount, Type = "INCOME", Date = DateTime.UtcNow };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(dto, 10));
        }

        [Fact]
        public async Task CreateTransaction_AccountNotOwnedByUser_ThrowsArgumentException()
        {
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync((Account?)null);

            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 100m, Type = "INCOME", Date = DateTime.UtcNow };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateTransaction(dto, 10));
        }

        // --- UpdateTransaction: revert-then-apply ---

        [Fact]
        public async Task UpdateTransaction_ExpenseAmountEdited_RevertsOldThenAppliesNew()
        {
            // Caso del documento: gasto de 23000 editado a 25000 en cuenta con 44000 -> resultado 42000
            var account = MakeAccount(1, userId: 10, balance: 44000m);
            var existing = MakeTransaction(id: 5, accountId: 1, type: "EXPENSE", amount: 23000m);

            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync(existing);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            var dto = new UpdateTransactionDto { CategoryId = 1, Amount = 25000m, Date = DateTime.UtcNow };

            await _service.UpdateTransaction(5, dto, 10);

            Assert.Equal(42000m, account.Balance);
        }

        [Fact]
        public async Task UpdateTransaction_IncomeAmountEdited_RevertsOldThenAppliesNew()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            var existing = MakeTransaction(id: 5, accountId: 1, type: "INCOME", amount: 200m);

            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync(existing);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            var dto = new UpdateTransactionDto { CategoryId = 1, Amount = 500m, Date = DateTime.UtcNow };

            await _service.UpdateTransaction(5, dto, 10);

            // 1000 - 200 (revert) + 500 (apply) = 1300
            Assert.Equal(1300m, account.Balance);
        }

        [Fact]
        public async Task UpdateTransaction_InvalidAmount_ThrowsArgumentException()
        {
            var dto = new UpdateTransactionDto { CategoryId = 1, Amount = 0, Date = DateTime.UtcNow };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateTransaction(5, dto, 10));
        }

        [Fact]
        public async Task UpdateTransaction_NotFound_ReturnsNull()
        {
            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync((Transaction?)null);

            var dto = new UpdateTransactionDto { CategoryId = 1, Amount = 100m, Date = DateTime.UtcNow };

            var result = await _service.UpdateTransaction(5, dto, 10);

            Assert.Null(result);
        }

        // --- DeleteTransaction: revierte el efecto ---

        [Fact]
        public async Task DeleteTransaction_Income_SubtractsFromBalance()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            var existing = MakeTransaction(id: 5, accountId: 1, type: "INCOME", amount: 300m);

            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync(existing);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            var result = await _service.DeleteTransaction(5, 10);

            Assert.True(result);
            Assert.Equal(700m, account.Balance);
            Assert.False(existing.IsActive);
        }

        [Fact]
        public async Task DeleteTransaction_Expense_AddsBackToBalance()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            var existing = MakeTransaction(id: 5, accountId: 1, type: "EXPENSE", amount: 300m);

            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync(existing);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            var result = await _service.DeleteTransaction(5, 10);

            Assert.True(result);
            Assert.Equal(1300m, account.Balance);
        }

        [Fact]
        public async Task DeleteTransaction_NotFound_ReturnsFalse()
        {
            _transactionRepository.Setup(r => r.GetTransactionById(5, 10)).ReturnsAsync((Transaction?)null);

            var result = await _service.DeleteTransaction(5, 10);

            Assert.False(result);
        }

        // --- Regresión: Create debe setear IsActive = true ---

        [Fact]
        public async Task CreateTransaction_SetsIsActiveTrue()
        {
            var account = MakeAccount(1, userId: 10, balance: 1000m);
            _accountRepository.Setup(r => r.GetAccountById(1, 10)).ReturnsAsync(account);

            Transaction? created = null;
            _transactionRepository.Setup(r => r.CreateTransaction(It.IsAny<Transaction>()))
                .Callback<Transaction>(t => { t.Id = 99; created = t; })
                .ReturnsAsync((Transaction t) => t);
            _transactionRepository.Setup(r => r.GetTransactionById(99, 10))
                .ReturnsAsync(() => created);

            var dto = new CreateTransactionDto { AccountId = 1, CategoryId = 1, Amount = 100m, Type = "INCOME", Date = DateTime.UtcNow };

            await _service.CreateTransaction(dto, 10);

            Assert.NotNull(created);
            Assert.True(created!.IsActive);
        }
    }
}
