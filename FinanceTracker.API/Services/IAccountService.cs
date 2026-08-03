using FinanceTracker.API.DTOs.Account;

namespace FinanceTracker.API.Services
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountResponseDto>> GetAllAccountsByUserId(int userId);
        Task<AccountResponseDto?> GetAccountById(int id, int userId);
        Task<AccountResponseDto> CreateAccount(CreateAccountDto dto, int userId);
        Task<AccountResponseDto?> UpdateAccount(int id, UpdateAccountDto dto, int userId);
        Task<bool> DeleteAccount(int id, int userId);
    }
}
