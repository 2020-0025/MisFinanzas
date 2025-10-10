using MisFinanzas.Domain.DTOs;


namespace MisFinanzas.Infrastructure.Interfaces
{
        public interface IFinancialGoalService
        {
            Task<List<FinancialGoalDto>> GetAllAsync(string userId);
            Task<FinancialGoalDto?> GetByIdAsync(int goalId, string userId);
            Task<FinancialGoalDto> CreateAsync(FinancialGoalDto goalDto, string userId);
            Task<FinancialGoalDto> UpdateAsync(FinancialGoalDto goalDto, string userId);
            Task<bool> DeleteAsync(int goalId, string userId);
            Task<bool> AddProgressAsync(int goalId, string userId, decimal amount);
            Task<List<FinancialGoalDto>> GetActiveGoalsAsync(string userId);
            Task<int> GetCompletedGoalsCountAsync(string userId);
        }
    
}
