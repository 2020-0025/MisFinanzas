using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;

        public BudgetService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BudgetDto>> GetAllByUserAsync(string userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();

            return budgets.Select(MapToDto).ToList();
        }

        public async Task<List<BudgetDto>> GetActiveByUserAsync(string userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.IsActive)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();

            return budgets.Select(MapToDto).ToList();
        }

        public async Task<List<BudgetDto>> GetByUserAndPeriodAsync(string userId, int month, int year)
        {
            var budgets = await _context.Budgets
        .Include(b => b.Category)
        .Where(b => b.UserId == userId && b.Month == month && b.Year == year && b.IsActive)
        .ToListAsync();

            // Calcular gastos reales por categoría
            var spentByCategory = await CalculateSpentByCategoryAsync(userId, month, year);

            // Mapear y asignar SpentAmount real
            return budgets.Select(b =>
            {
                var dto = MapToDto(b);

                // Asignar el gasto real calculado
                dto.SpentAmount = spentByCategory.GetValueOrDefault(b.CategoryId, 0);

                // Recalcular propiedades dependientes
                dto.AvailableAmount = dto.AssignedAmount - dto.SpentAmount;
                dto.UsedPercentage = dto.AssignedAmount > 0 ? (dto.SpentAmount / dto.AssignedAmount) * 100 : 0;
                dto.IsOverBudget = dto.SpentAmount > dto.AssignedAmount;
                dto.IsNearLimit = dto.UsedPercentage >= 80 && dto.UsedPercentage < 100;

                return dto;
            }).ToList();
        }

        public async Task<BudgetDto?> GetByIdAsync(int id, string userId)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            return budget != null ? MapToDto(budget) : null;
        }

        public async Task<(bool Success, string? Error, BudgetDto? Budget)> CreateAsync(BudgetDto dto, string userId)
        {
            // Validar que la categoría exista
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == dto.CategoryId && c.UserId == userId);

            if (!categoryExists)
            {
                return (false, "La categoría seleccionada no existe", null);
            }

            // Validar que no exista otro presupuesto activo para la misma categoría en el mismo periodo
            var existingBudget = await _context.Budgets
                .AnyAsync(b => b.UserId == userId &&
                              b.CategoryId == dto.CategoryId &&
                              b.Month == dto.Month &&
                              b.Year == dto.Year &&
                              b.IsActive);

            if (existingBudget)
            {
                return (false, "Ya existe un presupuesto activo para esta categoría en este periodo", null);
            }

            var budget = new Budget
            {
                UserId = userId,
                Name = dto.Name,
                AssignedAmount = dto.AssignedAmount,
                SpentAmount = 0,
                Month = dto.Month,
                Year = dto.Year,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return (true, null, MapToDto(budget));
        }

        public async Task<bool> UpdateAsync(int id, BudgetDto dto, string userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
                return false;

            budget.Name = dto.Name;
            budget.AssignedAmount = dto.AssignedAmount;
            budget.Month = dto.Month;
            budget.Year = dto.Year;
            budget.CategoryId = dto.CategoryId;
            budget.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
                return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegisterExpenseInBudgetAsync(int budgetId, decimal amount, string userId)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == budgetId && b.UserId == userId);

            if (budget == null)
                return false;

            budget.SpentAmount += amount;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalAvailableAmountAsync(string userId, int month, int year)
        {
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && b.Month == month && b.Year == year && b.IsActive)
                .ToListAsync();

            return budgets.Sum(b => b.AvailableAmount);
        }

        public async Task<bool> ValidateAvailableBudgetAsync(string userId, int month, int year, decimal requiredAmount)
        {
            var availableAmount = await GetTotalAvailableAmountAsync(userId, month, year);
            return availableAmount >= requiredAmount;
        }

        public async Task<List<BudgetDto>> GetBudgetsByCategoryAsync(string userId, int month, int year)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId &&
                           b.Month == month &&
                           b.Year == year &&
                           b.IsActive)
                .ToListAsync();

            return budgets.Select(MapToDto).ToList();
        }

        public async Task<decimal> GetTotalBudgetForMonthAsync(string userId, int month, int year)
        {
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId &&
                            b.Month == month &&
                            b.Year == year &&
                            b.IsActive)
                .ToListAsync();

            return budgets.Sum(b => b.AssignedAmount);
        }

        public async Task<decimal> GetTotalSpentForMonthAsync(string userId, int month, int year)
        {
            // CAMBIO PRINCIPAL: Solo contar gastos de categorías con presupuesto asignado
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Obtener IDs de categorías que tienen presupuesto activo en este periodo
            var categoriesWithBudget = await _context.Budgets
                .Where(b => b.UserId == userId &&
                            b.Month == month &&
                            b.Year == year &&
                            b.IsActive)
                .Select(b => b.CategoryId)
                .ToListAsync();

            // Solo sumar gastos de esas categorías
            var expenses = await _context.ExpensesIncomes
                .Where(e => e.UserId == userId &&
                            e.Type == Domain.Enums.TransactionType.Expense &&
                            e.Date >= firstDay &&
                            e.Date <= lastDay &&
                            categoriesWithBudget.Contains(e.CategoryId)) // FILTRO CLAVE
                .ToListAsync();

            return expenses.Sum(e => e.Amount);
        }

        private async Task<Dictionary<int, decimal>> CalculateSpentByCategoryAsync(string userId, int month, int year)
        {
            // Calcular gastos reales desde ExpensesIncomes
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var expenses = await _context.ExpensesIncomes
                .Where(e => e.UserId == userId &&
                            e.Type == Domain.Enums.TransactionType.Expense &&
                            e.Date >= firstDay &&
                            e.Date <= lastDay)
                .ToListAsync();

            return expenses
                .GroupBy(e => e.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        // ========== MÉTODOS PARA DASHBOARD ==========

        public async Task<List<BudgetDto>> GetBudgetsForChartAsync(string userId, int month, int year)
        {
            // Reutilizamos GetByUserAndPeriodAsync que ya calcula SpentAmount dinámicamente
            return await GetByUserAndPeriodAsync(userId, month, year);
        }

        public async Task<List<BudgetDto>> GetExceededBudgetsAsync(string userId, int month, int year)
        {
            var budgets = await GetByUserAndPeriodAsync(userId, month, year);
            return budgets.Where(b => b.IsOverBudget || b.IsNearLimit).ToList();
        }

        // Helper para mapear Budget a BudgetDto
        private static BudgetDto MapToDto(Budget budget)
        {
            return new BudgetDto
            {
                Id = budget.Id,
                UserId = budget.UserId,
                Name = budget.Name,
                AssignedAmount = budget.AssignedAmount,
                SpentAmount = budget.SpentAmount,
                AvailableAmount = budget.AvailableAmount,
                UsedPercentage = budget.UsedPercentage,
                IsOverBudget = budget.IsOverBudget,
                IsNearLimit = budget.IsNearLimit,
                Month = budget.Month,
                Year = budget.Year,
                CategoryId = budget.CategoryId,
                CategoryTitle = budget.Category?.Title ?? "Sin categoría",
                CategoryIcon = budget.Category?.Icon ?? "📁",
                IsActive = budget.IsActive,
                CreatedAt = budget.CreatedAt
            };
        }

        public async Task<(bool Success, string? Error, int CopiedCount)> CopyBudgetsFromPreviousMonthAsync(string userId, int targetMonth, int targetYear)
        {
            try
            {
                // Calcular mes anterior
                var previousMonth = targetMonth - 1;
                var previousYear = targetYear;

                if (previousMonth == 0)
                {
                    previousMonth = 12;
                    previousYear = targetYear - 1;
                }

                // Obtener presupuestos del mes anterior
                var previousBudgets = await _context.Budgets
                    .Include(b => b.Category)
                    .Where(b => b.UserId == userId &&
                               b.Month == previousMonth &&
                               b.Year == previousYear &&
                               b.IsActive)
                    .ToListAsync();

                if (!previousBudgets.Any())
                {
                    return (false, "No hay presupuestos en el mes anterior para copiar", 0);
                }

                // Verificar si ya existen presupuestos en el mes objetivo
                var existingBudgets = await _context.Budgets
                    .AnyAsync(b => b.UserId == userId &&
                                  b.Month == targetMonth &&
                                  b.Year == targetYear);

                if (existingBudgets)
                {
                    return (false, "Ya existen presupuestos en este mes. Elimina los existentes primero.", 0);
                }

                // Crear nuevos presupuestos para el mes objetivo
                var newBudgets = previousBudgets.Select(pb => new Budget
                {
                    UserId = userId,
                    Name = pb.Name,
                    AssignedAmount = pb.AssignedAmount,
                    SpentAmount = 0,
                    Month = targetMonth,
                    Year = targetYear,
                    CategoryId = pb.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.Budgets.AddRange(newBudgets);
                await _context.SaveChangesAsync();

                return (true, null, newBudgets.Count);
            }
            catch (Exception ex)
            {
                return (false, $"Error al copiar presupuestos: {ex.Message}", 0);
            }
        }
    }
}