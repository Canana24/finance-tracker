using FinanceTracker.API.Data;
using FinanceTracker.API.DTOs.Transaction;
using FinanceTracker.API.Models;
using FinanceTracker.API.Repositories;
using System.Runtime.ConstrainedExecution;

namespace FinanceTracker.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly FinanceTrackerContext _context;

        public TransactionService(ITransactionRepository transactionRepository, IAccountRepository accountRepository, FinanceTrackerContext context)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _context = context;
        }

        public async Task <IEnumerable<TransactionResponseDto>> GetAllTransactionsByUserId(int userId)
        {
           var transactions = await _transactionRepository.GetAllTransactionsByUserId(userId);
           return transactions.Select(MapToResponseDto);
        }

        public async Task<TransactionResponseDto?> GetTransactionById(int id, int userId)
        {
            var transaction = await _transactionRepository.GetTransactionById(id, userId);
            return transaction == null ? null : MapToResponseDto(transaction);
        }

        public async Task<TransactionResponseDto> CreateTransaction(CreateTransactionDto dto, int userId)
        {
            if(dto.Type != "INCOME" && dto.Type != "EXPENSE")
            {
                throw new ArgumentException("El tipo debe ser INCOME o EXPENSE.");
            }

            if(dto.Amount <= 0)
            {
                throw new ArgumentException("El monto debe ser mayor a cero.");
            }

            var account = await _accountRepository.GetAccountById(dto.AccountId, userId);
            if (account == null) 
            { 
                throw new ArgumentException("La cuenta no existe o no esta asociada al usuario."); 
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = new Transaction
                {
                    AccountId = dto.AccountId,
                    CategoryId = dto.CategoryId,
                    CurrencyId = account.CurrencyId,
                    Amount = dto.Amount,
                    Type = dto.Type,
                    Description = dto.Description,
                    Date = dto.Date,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    IsActive = true
                };

                await _transactionRepository.CreateTransaction(transaction);

                if(dto.Type == "INCOME") { 
                    account.Balance += dto.Amount;
                }
                else
                {
                    account.Balance -= dto.Amount;
                }

                account.UpdatedAt = DateTime.UtcNow;
                account.UpdatedBy = userId;

                await _accountRepository.UpdateAccount(account);

                await dbTransaction.CommitAsync();

                var fullTransaction = await _transactionRepository.GetTransactionById(transaction.Id, userId);
                return MapToResponseDto(fullTransaction);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteTransaction(int id, int userId)
        {
            var transaction = await _transactionRepository.GetTransactionById(id, userId);
            if (transaction == null)
            {
                return false;
            }

            var account = await _accountRepository.GetAccountById(transaction.AccountId, userId);
            if (account == null)
            {
                return false;
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (transaction.Type == "INCOME")
                {
                    account.Balance -= transaction.Amount;
                }
                else
                {
                    account.Balance += transaction.Amount;
                }

                account.UpdatedAt = DateTime.UtcNow;
                account.UpdatedBy = userId;
                await _accountRepository.UpdateAccount(account);

                transaction.IsActive = false;
                transaction.DeletedAt = DateTime.UtcNow;
                transaction.DeletedBy = userId;
                await _transactionRepository.UpdateTransaction(transaction);

                await dbTransaction.CommitAsync();
                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;

            }
        }

        public async Task<TransactionResponseDto?> UpdateTransaction(int id, UpdateTransactionDto dto, int userId)
        {
            if (dto.Amount <= 0) 
            { 
                throw new ArgumentException("El monto debe ser mayor a cero.");
            }

            var transaction = await _transactionRepository.GetTransactionById(id, userId);
            if (transaction == null)
            {
                return null;
            }
            
            var account = await _accountRepository.GetAccountById(transaction.AccountId, userId);
            if(account == null)
            {
                return null;
            }

            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if(transaction.Type == "INCOME")
                {
                    account.Balance -= transaction.Amount;
                }
                
                else
                {
                    account.Balance += transaction.Amount;
                }
                
                transaction.CategoryId = dto.CategoryId;
                transaction.Amount = dto.Amount;
                transaction.Description = dto.Description;
                transaction.Date = dto.Date;
                transaction.UpdatedAt = DateTime.UtcNow;
                transaction.UpdatedBy = userId;

                if (transaction.Type == "INCOME")
                {
                    account.Balance += dto.Amount;
                }
                else
                {
                    account.Balance -= dto.Amount;
                }

                account.UpdatedAt = DateTime.UtcNow;
                account.UpdatedBy = userId;

                await _transactionRepository.UpdateTransaction(transaction);
                await _accountRepository.UpdateAccount(account);

                await dbTransaction.CommitAsync();

                var fullTransaction = await _transactionRepository.GetTransactionById(transaction.Id, userId);
                return MapToResponseDto(fullTransaction!);
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }

        }

        private static TransactionResponseDto MapToResponseDto(Transaction transaction)
        {
            return new TransactionResponseDto
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                AccountName = transaction.Account?.Name ?? string.Empty,
                CategoryName = transaction.Category?.Name ?? string.Empty,
                Amount = transaction.Amount,
                Type = transaction.Type,
                Description = transaction.Description,
                Date = transaction.Date
            };
        }
    }
}
