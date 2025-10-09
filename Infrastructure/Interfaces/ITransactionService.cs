using MisFinanzas.Domain.DTOs;


namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ITransactionService
    {
        Task<List<TransactionDto>> GetAllAsync(string userId);
        Task<TransactionDto?> GetByIdAsync(int transactionId, string userId);
        Task<TransactionDto> CreateAsync(TransactionDto transactionDto, string userId);
        Task<TransactionDto> UpdateAsync(TransactionDto transactionDto, string userId);
        Task<bool> DeleteAsync(int transactionId, string userId);

        // Métodos para el Dashboard
        Task<decimal> GetTotalIncomeAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpenseAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetBalanceAsync(string userId);
        Task<List<TransactionDto>> GetRecentAsync(string userId, int count = 10);
        Task<List<TransactionDto>> GetByDateRangeAsync(string userId, DateTime startDate, DateTime endDate);

    }
}
