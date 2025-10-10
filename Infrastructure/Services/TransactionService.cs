using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
        public class TransactionService : ITransactionService
        {
            private readonly ApplicationDbContext _context;

            public TransactionService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<List<TransactionDto>> GetAllAsync(string userId)
            {
                return await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        Amount = t.Amount,
                        Note = t.Note,
                        Date = t.Date,
                        CategoryName = t.Category!.Title,
                        CategoryIcon = t.Category.Icon
                    })
                    .ToListAsync();
            }

            public async Task<TransactionDto?> GetByIdAsync(int transactionId, string userId)
            {
                return await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.TransactionId == transactionId && t.UserId == userId)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        Amount = t.Amount,
                        Note = t.Note,
                        Date = t.Date,
                        CategoryName = t.Category!.Title,
                        CategoryIcon = t.Category.Icon
                    })
                    .FirstOrDefaultAsync();
            }

            public async Task<TransactionDto> CreateAsync(TransactionDto transactionDto, string userId)
            {
                var transaction = new Transaction
                {
                    CategoryId = transactionDto.CategoryId,
                    Amount = transactionDto.Amount,
                    Note = transactionDto.Note,
                    Date = transactionDto.Date,
                    UserId = userId
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                transactionDto.TransactionId = transaction.TransactionId;

                // Cargar datos de categoría para el DTO
                var category = await _context.Categories.FindAsync(transactionDto.CategoryId);
                if (category != null)
                {
                    transactionDto.CategoryName = category.Title;
                    transactionDto.CategoryIcon = category.Icon;
                }

                return transactionDto;
            }

            public async Task<TransactionDto> UpdateAsync(TransactionDto transactionDto, string userId)
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionDto.TransactionId && t.UserId == userId);

                if (transaction == null)
                    throw new InvalidOperationException("Transacción no encontrada");

                transaction.CategoryId = transactionDto.CategoryId;
                transaction.Amount = transactionDto.Amount;
                transaction.Note = transactionDto.Note;
                transaction.Date = transactionDto.Date;

                await _context.SaveChangesAsync();

                // Cargar datos de categoría para el DTO
                var category = await _context.Categories.FindAsync(transactionDto.CategoryId);
                if (category != null)
                {
                    transactionDto.CategoryName = category.Title;
                    transactionDto.CategoryIcon = category.Icon;
                }

                return transactionDto;
            }

            public async Task<bool> DeleteAsync(int transactionId, string userId)
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId);

                if (transaction == null)
                    return false;

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return true;
            }

            public async Task<decimal> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
            {
                var query = _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.Category!.Type == TransactionType.Income);

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.Date <= endDate.Value);

                return (decimal)await query.SumAsync(t => (double)t.Amount);
            }

            public async Task<decimal> GetTotalExpenseAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
            {
                var query = _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.Category!.Type == TransactionType.Expense);

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.Date <= endDate.Value);

                return (decimal)await query.SumAsync(t => (double)t.Amount);
            }

            public async Task<decimal> GetBalanceAsync(string userId)
            {
                var income = await GetTotalIncomeAsync(userId);
                var expense = await GetTotalExpenseAsync(userId);
                return income - expense;
            }

            public async Task<List<TransactionDto>> GetRecentAsync(string userId, int count = 10)
            {
                return await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .Take(count)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        Amount = t.Amount,
                        Note = t.Note,
                        Date = t.Date,
                        CategoryName = t.Category!.Title,
                        CategoryIcon = t.Category.Icon
                    })
                    .ToListAsync();
            }

            public async Task<List<TransactionDto>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
            {
                return await _context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                    .OrderByDescending(t => t.Date)
                    .Select(t => new TransactionDto
                    {
                        TransactionId = t.TransactionId,
                        CategoryId = t.CategoryId,
                        Amount = t.Amount,
                        Note = t.Note,
                        Date = t.Date,
                        CategoryName = t.Category!.Title,
                        CategoryIcon = t.Category.Icon
                    })
                    .ToListAsync();
            }
        }
    
}
