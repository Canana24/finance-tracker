using FinanceTracker.API.DTOs.Account;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;

namespace FinanceTracker.API.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<AccountResponseDto>> GetAllAccountsByUserId(int userId)
        {
            var accounts = await _accountRepository.GetAllAccountsByUserId(userId);
            return accounts.Select(MapToResponseDto);
        }

        public async Task<AccountResponseDto?> GetAccountById(int id, int userId)
        {
            var account = await _accountRepository.GetAccountById(id, userId);
            return account == null ? null : MapToResponseDto(account);
        }

        public async Task<AccountResponseDto> CreateAccount(CreateAccountDto dto, int userId)
        {
            var account = new Account
            {
                UserId = userId,
                CurrencyId = dto.CurrencyId,
                Name = dto.Name,
                Balance = dto.InitialBalance,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsActive = true
            };

            var created = await _accountRepository.CreateAccount(account);

            // Traigo la Currency incluida
            var fullAccount = await _accountRepository.GetAccountById(created.Id, userId);
            return MapToResponseDto(fullAccount!);
        }

        public async Task<AccountResponseDto?> UpdateAccount(int id, UpdateAccountDto dto, int userId)
        {
            var account = await _accountRepository.GetAccountById(id, userId);

            if (account == null)
                return null;

            account.Name = dto.Name;
            account.CurrencyId = dto.CurrencyId;
            account.UpdatedAt = DateTime.UtcNow;
            account.UpdatedBy = userId;

            await _accountRepository.UpdateAccount(account);

            var updated = await _accountRepository.GetAccountById(id, userId);
            return MapToResponseDto(updated!);
        }

        public async Task<bool> DeleteAccount(int id, int userId)
        {
            var account = await _accountRepository.GetAccountById(id, userId);

            if (account == null)
                return false;

            // Borrado lógico
            account.IsActive = false;
            account.DeletedAt = DateTime.UtcNow;
            account.DeletedBy = userId;

            await _accountRepository.UpdateAccount(account);
            return true;
        }

        private static AccountResponseDto MapToResponseDto(Account account)
        {
            return new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                Balance = account.Balance,
                CurrencyCode = account.Currency?.Code ?? string.Empty,
                CurrencySymbol = account.Currency?.Symbol ?? string.Empty,
                CreatedAt = account.CreatedAt
            };
        }
    }
}
