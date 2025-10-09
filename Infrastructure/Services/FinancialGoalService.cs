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

            public async Task<List<FinancialGoalDto>> GetAllAsync(string userId)
            {
                return await _context.FinancialGoals
                    .Where(g => g.UserId == userId)
                    .OrderBy(g => g.TargetDate)
                    .Select(g => MapToDto(g))
                    .ToListAsync();
            }

            public async Task<FinancialGoalDto?> GetByIdAsync(int goalId, string userId)
            {
                var goal = await _context.FinancialGoals
                    .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

                return goal != null ? MapToDto(goal) : null;
            }

            public async Task<FinancialGoalDto> CreateAsync(FinancialGoalDto goalDto, string userId)
            {
                var goal = new FinancialGoal
                {
                    Title = goalDto.Title,
                    Description = goalDto.Description,
                    TargetAmount = goalDto.TargetAmount,
                    CurrentAmount = goalDto.CurrentAmount,
                    StartDate = goalDto.StartDate,
                    TargetDate = goalDto.TargetDate,
                    Status = goalDto.Status,
                    Icon = goalDto.Icon,
                    UserId = userId
                };

                _context.FinancialGoals.Add(goal);
                await _context.SaveChangesAsync();

                goalDto.GoalId = goal.GoalId;
                return MapToDto(goal);
            }

            public async Task<FinancialGoalDto> UpdateAsync(FinancialGoalDto goalDto, string userId)
            {
                var goal = await _context.FinancialGoals
                    .FirstOrDefaultAsync(g => g.GoalId == goalDto.GoalId && g.UserId == userId);

                if (goal == null)
                    throw new InvalidOperationException("Meta no encontrada");

                goal.Title = goalDto.Title;
                goal.Description = goalDto.Description;
                goal.TargetAmount = goalDto.TargetAmount;
                goal.CurrentAmount = goalDto.CurrentAmount;
                goal.StartDate = goalDto.StartDate;
                goal.TargetDate = goalDto.TargetDate;
                goal.Status = goalDto.Status;
                goal.Icon = goalDto.Icon;

                // Actualizar estado automáticamente
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.Status = GoalStatus.Completed;
                }

                await _context.SaveChangesAsync();

                return MapToDto(goal);
            }

            public async Task<bool> DeleteAsync(int goalId, string userId)
            {
                var goal = await _context.FinancialGoals
                    .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

                if (goal == null)
                    return false;

                _context.FinancialGoals.Remove(goal);
                await _context.SaveChangesAsync();

                return true;
            }

            public async Task<bool> AddProgressAsync(int goalId, string userId, decimal amount)
            {
                var goal = await _context.FinancialGoals
                    .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);

                if (goal == null)
                    return false;

                goal.CurrentAmount += amount;

                // Actualizar estado si se completó
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.Status = GoalStatus.Completed;
                }

                await _context.SaveChangesAsync();

                return true;
            }

            public async Task<List<FinancialGoalDto>> GetActiveGoalsAsync(string userId)
            {
                return await _context.FinancialGoals
                    .Where(g => g.UserId == userId && g.Status == GoalStatus.InProgress)
                    .OrderBy(g => g.TargetDate)
                    .Select(g => MapToDto(g))
                    .ToListAsync();
            }

            public async Task<int> GetCompletedGoalsCountAsync(string userId)
            {
                return await _context.FinancialGoals
                    .Where(g => g.UserId == userId && g.Status == GoalStatus.Completed)
                    .CountAsync();
            }

            // Helper method para mapear Entity a DTO
            private static FinancialGoalDto MapToDto(FinancialGoal goal)
            {
                return new FinancialGoalDto
                {
                    GoalId = goal.GoalId,
                    Title = goal.Title,
                    Description = goal.Description,
                    TargetAmount = goal.TargetAmount,
                    CurrentAmount = goal.CurrentAmount,
                    StartDate = goal.StartDate,
                    TargetDate = goal.TargetDate,
                    Status = goal.Status,
                    Icon = goal.Icon,
                    ProgressPercentage = goal.ProgressPercentage,
                    RemainingAmount = goal.RemainingAmount,
                    DaysRemaining = goal.DaysRemaining,
                    IsCompleted = goal.IsCompleted,
                    IsOverdue = goal.IsOverdue
                };
            }
        }
    
}
