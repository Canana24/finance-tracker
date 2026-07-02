using FinanceTracker.API.DTOs.Account;
using FinanceTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // Traigo UserId desde el token JWT
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _accountService.GetAllAccountsByUserId(GetUserId());
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _accountService.GetAccountById(id, GetUserId());

            if (account == null)
                return NotFound(new { message = $"No se encontró la cuenta con Id {id}." });

            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
        {
            var account = await _accountService.CreateAccount(dto, GetUserId());
            return CreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, UpdateAccountDto dto)
        {
            var account = await _accountService.UpdateAccount(id, dto, GetUserId());

            if (account == null)
                return NotFound(new { message = $"No se encontró la cuenta con Id {id}." });

            return Ok(account);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _accountService.DeleteAccount(id, GetUserId());

            if (!deleted)
                return NotFound(new { message = $"No se encontró la cuenta con Id {id}." });

            return Ok(new { message = "Cuenta eliminada correctamente." });
        }
    }
}
