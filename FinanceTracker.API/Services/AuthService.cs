using FinanceTracker.API.Configuration;
using FinanceTracker.API.Data;
using FinanceTracker.API.DTOs.Auth;
using FinanceTracker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinanceTracker.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly FinanceTrackerContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(FinanceTrackerContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new Exception("La casillla de correo ya está registrada.");

            var userRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "USER");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = passwordHash,
                RoleId = userRole.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateToken(user, userRole.Name!);
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive == true);

            if (user == null)
                throw new Exception("Email o contraseña incorrectos.");

            var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

            if (!isValidPassword)
                throw new Exception("Email o contraseña incorrectos.");

            return GenerateToken(user, user.Role!.Name!);
        }

        private TokenResponseDto GenerateToken(User user, string role)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               issuer: _jwtSettings.Issuer,
               audience: _jwtSettings.Audience,
               claims: claims,
               expires: expiresAt,
               signingCredentials: credentials
            );

            return new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt
            };
        }
    }
}
