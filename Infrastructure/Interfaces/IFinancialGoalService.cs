using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface IFinancialGoalService
    {
        Task<List<FinancialGoalDto>> GetAllByUserAsync(string userId);
        Task<List<FinancialGoalDto>> GetActiveByUserAsync(string userId);
        Task<FinancialGoalDto?> GetByIdAsync(int id, string userId);
        Task<(bool Success, string? Error, FinancialGoalDto? Goal)> CreateAsync(FinancialGoalDto dto, string userId);
        Task<bool> UpdateAsync(int id, FinancialGoalDto dto, string userId);
        Task<bool> DeleteAsync(int id, string userId);

        // Lógica de negocio
        Task<(bool Success, string? Error)> AddProgressAsync(int goalId, decimal amount, string userId);
        Task<(bool Success, string? Error)> WithdrawAmountAsync(int goalId, decimal amount, string userId);
        Task<bool> CompleteGoalAsync(int goalId, string userId);
        Task<bool> CancelGoalAsync(int goalId, string userId);
        Task<bool> ReactivateGoalAsync(int goalId, string userId);

        // Estadísticas
        Task<int> GetCompletedGoalsCountAsync(string userId);

        // Para Dashboard
        Task<List<FinancialGoalDto>> GetTopGoalsByProgressAsync(string userId, int count);
        Task<int> GetActiveGoalsCountAsync(string userId);

        // Validaciones
        Task<bool> ExistsGoalWithNameAsync(string title, string userId, int? excludeGoalId = null);
        Task<bool> ExistsGoalWithIconAsync(string icon, string userId, int? excludeGoalId = null);
    }
}