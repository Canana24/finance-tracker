using FinanceTracker.API.DTOs.Auth;

namespace FinanceTracker.API.Services
{
    public interface IAuthService
    {
            Task<TokenResponseDto> RegisterAsync(RegisterDto dto);
            Task<TokenResponseDto> LoginAsync(LoginDto dto);   
    }
}
