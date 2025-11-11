using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IBudgetService
    {
        // Consultas básicas
        Task<List<BudgetDto>> GetAllByUserAsync(string userId);
        Task<List<BudgetDto>> GetActiveByUserAsync(string userId);
        Task<List<BudgetDto>> GetByUserAndPeriodAsync(string userId, int month, int year);
        Task<BudgetDto?> GetByIdAsync(int id, string userId);

        // CRUD
        Task<(bool Success, string? Error, BudgetDto? Budget)> CreateAsync(BudgetDto dto, string userId);
        Task<bool> UpdateAsync(int id, BudgetDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Lógica de negocio
        Task<bool> RegisterExpenseInBudgetAsync(int budgetId, decimal amount, string userId);
        Task<decimal> GetTotalAvailableAmountAsync(string userId, int month, int year);
        Task<bool> ValidateAvailableBudgetAsync(string userId, int month, int year, decimal requiredAmount);

        // Para gráficos
        Task<List<BudgetDto>> GetBudgetsByCategoryAsync(string userId, int month, int year);

        Task<decimal> GetTotalBudgetForMonthAsync(string userId, int month, int year);
        Task<decimal> GetTotalSpentForMonthAsync(string userId, int month, int year);

        // Para Dashboard
        Task<List<BudgetDto>> GetBudgetsForChartAsync(string userId, int month, int year);
        Task<List<BudgetDto>> GetExceededBudgetsAsync(string userId, int month, int year);
    }
}