using MisFinanzas.Domain.Entities;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface ILoanService
    {
        // Consultas básicas
        Task<List<Loan>> GetAllByUserAsync(string userId);
        Task<List<Loan>> GetActiveByUserAsync(string userId);
        Task<Loan?> GetByIdAsync(int loanId, string userId);

        // CRUD
        Task<(bool Success, string? Error, Loan? Loan)> CreateAsync(Loan loan, string userId, bool createReminder = false);
        Task<bool> UpdateAsync(int loanId, Loan loan, string userId);
        Task<bool> DeleteAsync(int loanId, string userId, bool deleteHistory = false);

        // Operaciones específicas de préstamos
        Task<bool> RegisterPaymentAsync(int loanId, string userId);
        Task<bool> UndoLastPaymentAsync(int loanId, string userId);
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
        Task<List<Loan>> GetLoansWithUpcomingPaymentsAsync(string userId, int daysAhead = 7);
    }
}