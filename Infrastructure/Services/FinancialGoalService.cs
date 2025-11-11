using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class FinancialGoalService : IFinancialGoalService
    {
        private readonly ApplicationDbContext _context;

        public FinancialGoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FinancialGoalDto>> GetAllByUserAsync(string userId)
        {
            return await _context.FinancialGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .Select(g => MapToDto(g))
                .ToListAsync();
        }

        public async Task<List<FinancialGoalDto>> GetActiveByUserAsync(string userId)
        {
            return await _context.FinancialGoals
                .Where(g => g.UserId == userId && g.Status == GoalStatus.InProgress)
                .OrderBy(g => g.TargetDate)
                .Select(g => MapToDto(g))
                .ToListAsync();
        }

        public async Task<FinancialGoalDto?> GetByIdAsync(int id, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == id && g.UserId == userId);

            return goal != null ? MapToDto(goal) : null;
        }

        public async Task<(bool Success, string? Error, FinancialGoalDto? Goal)> CreateAsync(FinancialGoalDto dto, string userId)
        {
            var goal = new FinancialGoal
            {
                Title = dto.Title,
                Description = dto.Description,
                TargetAmount = dto.TargetAmount,
                CurrentAmount = 0,
                StartDate = dto.StartDate,
                TargetDate = dto.TargetDate,
                Status = GoalStatus.InProgress,
                Icon = dto.Icon ?? "🎯",
                UserId = userId
            };

            _context.FinancialGoals.Add(goal);
            await _context.SaveChangesAsync();

            return (true, null, MapToDto(goal));
        }

        public async Task<bool> UpdateAsync(int id, FinancialGoalDto dto, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == id && g.UserId == userId);

            if (goal == null)
                return false;

            goal.Title = dto.Title;
            goal.Description = dto.Description;
            goal.TargetAmount = dto.TargetAmount;
            goal.StartDate = dto.StartDate;
            goal.TargetDate = dto.TargetDate;
            goal.Icon = dto.Icon ?? goal.Icon;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == id && g.UserId == userId);

            if (goal == null)
                return false;

            _context.FinancialGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Error)> AddProgressAsync(int goalId, decimal amount, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

            if (goal == null)
                return (false, "Meta no encontrada");

            if (goal.Status != GoalStatus.InProgress)
                return (false, "La meta no está en progreso");

            if (amount <= 0)
                return (false, "El monto debe ser mayor a cero");

            goal.CurrentAmount += amount;

            // Verificar si se completó la meta
            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.Status = GoalStatus.Completed;
                goal.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> WithdrawAmountAsync(int goalId, decimal amount, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

            if (goal == null)
                return (false, "Meta no encontrada");

            if (amount <= 0)
                return (false, "El monto debe ser mayor a cero");

            if (amount > goal.CurrentAmount)
                return (false, "No puedes retirar más de lo que hay en la meta");

            goal.CurrentAmount -= amount;

            // Si estaba completada y ahora no, cambiar estado
            if (goal.Status == GoalStatus.Completed && goal.CurrentAmount < goal.TargetAmount)
            {
                goal.Status = GoalStatus.InProgress;
                goal.CompletedAt = null;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> CompleteGoalAsync(int goalId, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

            if (goal == null || goal.Status != GoalStatus.InProgress)
                return false;

            goal.Status = GoalStatus.Completed;
            goal.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelGoalAsync(int goalId, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

            if (goal == null || goal.Status != GoalStatus.InProgress)
                return false;

            goal.Status = GoalStatus.Cancelled;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReactivateGoalAsync(int goalId, string userId)
        {
            var goal = await _context.FinancialGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

            if (goal == null || goal.Status != GoalStatus.Cancelled)
                return false;

            goal.Status = GoalStatus.InProgress;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCompletedGoalsCountAsync(string userId)
        {
            return await _context.FinancialGoals
                .Where(g => g.UserId == userId && g.Status == GoalStatus.Completed)
                .CountAsync();
        }

        // ========== MÉTODOS PARA DASHBOARD ==========

        public async Task<List<FinancialGoalDto>> GetTopGoalsByProgressAsync(string userId, int count)
        {
            var goals = await _context.FinancialGoals
                .Where(g => g.UserId == userId && g.Status == GoalStatus.InProgress)
                .OrderByDescending(g => g.ProgressPercentage)
                .Take(count)
                .ToListAsync();

            return goals.Select(MapToDto).ToList();
        }

        public async Task<int> GetActiveGoalsCountAsync(string userId)
        {
            return await _context.FinancialGoals
                .CountAsync(g => g.UserId == userId && g.Status == GoalStatus.InProgress);
        }

        public async Task<bool> ExistsGoalWithNameAsync(string title, string userId, int? excludeGoalId = null)
        {
            var query = _context.FinancialGoals
                .Where(g => g.UserId == userId
                    && g.Title.ToLower() == title.ToLower());

            if (excludeGoalId.HasValue)
            {
                query = query.Where(g => g.GoalId != excludeGoalId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsGoalWithIconAsync(string icon, string userId, int? excludeGoalId = null)
        {
            var query = _context.FinancialGoals
                .Where(g => g.UserId == userId
                    && g.Icon == icon);

            if (excludeGoalId.HasValue)
            {
                query = query.Where(g => g.GoalId != excludeGoalId.Value);
            }

            return await query.AnyAsync();
        }
        private static FinancialGoalDto MapToDto(FinancialGoal goal)
        {
            return new FinancialGoalDto
            {
                GoalId = goal.GoalId,
                UserId = goal.UserId,
                Title = goal.Title,
                Description = goal.Description,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                StartDate = goal.StartDate,
                TargetDate = goal.TargetDate,
                Status = goal.Status,
                Icon = goal.Icon,
                CompletedAt = goal.CompletedAt
            };
        }
    }
}
