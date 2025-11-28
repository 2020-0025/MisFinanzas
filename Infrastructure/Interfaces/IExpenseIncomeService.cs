using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IExpenseIncomeService
    {
        Task<List<ExpenseIncomeDto>> GetAllByUserAsync(string userId);
        Task<List<ExpenseIncomeDto>> GetByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task<List<ExpenseIncomeDto>> GetByUserAndTypeAsync(string userId, TransactionType type);
        Task<ExpenseIncomeDto?> GetByIdAsync(int id, string userId);
        Task<(bool Success, string? Error, ExpenseIncomeDto? Data)> CreateAsync(ExpenseIncomeDto dto, string userId);
        Task<bool> UpdateAsync(int id, ExpenseIncomeDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Cálculos
        Task<decimal> GetTotalIngresosByUserAsync(string userId);
        Task<decimal> GetTotalGastosByUserAsync(string userId);
        Task<decimal> GetTotalAdjustmentsByUserAsync(string userId);
        Task<decimal> GetBalanceByUserAsync(string userId);
        Task<decimal> GetIngresosMesActualAsync(string userId);
        Task<decimal> GetGastosMesActualAsync(string userId);

        // Para Dashboard - Gráfico de líneas
        Task<Dictionary<DateTime, decimal>> GetDailyIncomesAsync(string userId, DateTime startDate, DateTime endDate);
        Task<Dictionary<DateTime, decimal>> GetDailyExpensesAsync(string userId, DateTime startDate, DateTime endDate);

        // Para Dashboard - Últimas transacciones
        Task<List<ExpenseIncomeDto>> GetRecentTransactionsAsync(string userId, int count);

        // Para Dashboard - Comparativa mensual
        Task<(decimal Ingresos, decimal Gastos)> GetTotalsByMonthAsync(string userId, int month, int year);


    }
}