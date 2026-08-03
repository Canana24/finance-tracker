using FinanceTracker.API.DTOs.Transaction;
using FinanceTracker.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _transactionService.GetAllTransactionsByUserId(GetUserId());
            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            var transaction = await _transactionService.GetTransactionById(id, GetUserId());
            if (transaction == null)
                return NotFound(new { message = $"No se encontró la transacción con Id {id}." });
            return Ok(transaction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
        {
            var transaction = await _transactionService.CreateTransaction(dto, GetUserId());
            return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, UpdateTransactionDto dto)
        {
            var transaction = await _transactionService.UpdateTransaction(id, dto, GetUserId());
            if (transaction == null)
                return NotFound(new { message = $"No se encontró la transacción con Id {id}." });
            return Ok(transaction);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id) 
        { 
            var deleted = await _transactionService.DeleteTransaction(id, GetUserId());

            if(!deleted)
                return NotFound(new { message = $"No se encontró la transacción con Id {id}." });

            return Ok(new { message = $"Transacción eliminada correctamente." });
        }
    }
}
