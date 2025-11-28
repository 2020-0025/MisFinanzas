using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ILoanService
    {
        // Consultas básicas
        Task<List<LoanDto>> GetAllByUserAsync(string userId);
        Task<List<LoanDto>> GetActiveByUserAsync(string userId);
        Task<LoanDto?> GetByIdAsync(int loanId, string userId);

        // CRUD
        Task<(bool Success, string? Error, LoanDto? Loan)> CreateAsync(LoanDto loan, string userId, bool createReminder = false);
        Task<bool> UpdateAsync(int loanId, LoanDto loan, string userId);
        Task<bool> DeleteAsync(int loanId, string userId, bool deleteHistory = false);

        // Operaciones específicas de préstamos
        Task<bool> RegisterPaymentAsync(int loanId, string userId);
        Task<bool> UndoLastPaymentAsync(int loanId, string userId);
        Task<bool> AdjustBalanceAsync(int loanId, decimal newBalance, string userId);
        Task<bool> MarkAsCompletedAsync(int loanId, string userId);
        Task<bool> ReactivateLoanAsync(int loanId, string userId);

        // Validaciones
        Task<bool> ExistsLoanWithTitleAsync(string title, string userId, int? excludeLoanId = null);

        // Resúmenes y estadísticas
        Task<decimal> GetTotalBorrowedAsync(string userId);
        Task<decimal> GetTotalToPayAsync(string userId);
        Task<decimal> GetTotalPaidAsync(string userId);
        Task<decimal> GetTotalRemainingAsync(string userId);
        Task<decimal> GetMonthlyPaymentsTotalAsync(string userId);
        Task<decimal> GetAverageInterestRateAsync(string userId);

        // Para dashboard
        Task<List<LoanDto>> GetLoansWithUpcomingPaymentsAsync(string userId, int daysAhead = 7);
    }
}