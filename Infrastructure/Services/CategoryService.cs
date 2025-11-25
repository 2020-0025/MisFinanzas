using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        private readonly INotificationService _notificationService;

        public CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory, INotificationService notificationService)

        {
            _contextFactory = contextFactory;
            _notificationService = notificationService;
        }
        public async Task<List<CategoryDto>> GetAllByUserAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Categories

                .Where(c => c.UserId == userId)

                .OrderBy(c => c.Title)

                .Select(c => new CategoryDto

                {
                    CategoryId = c.CategoryId,

                    UserId = c.UserId,

                    Title = c.Title,

                    Icon = c.Icon,

                    Type = c.Type,

                    IsFixedExpense = c.IsFixedExpense,

                    DayOfMonth = c.DayOfMonth,

                    EstimatedAmount = c.EstimatedAmount
                })

                .ToListAsync();
        }

        public async Task<List<CategoryDto>> GetByUserAndTypeAsync(string userId, TransactionType type)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Categories

                .Where(c => c.UserId == userId && c.Type == type)

                .OrderBy(c => c.Title)

                .Select(c => new CategoryDto

                {
                    CategoryId = c.CategoryId,

                    UserId = c.UserId,

                    Title = c.Title,

                    Icon = c.Icon,

                    Type = c.Type,

                    IsFixedExpense = c.IsFixedExpense,

                    DayOfMonth = c.DayOfMonth,

                    EstimatedAmount = c.EstimatedAmount
                })

                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)
                return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                UserId = category.UserId,
                Title = category.Title,
                Icon = category.Icon,
                Type = category.Type,
                IsFixedExpense = category.IsFixedExpense,
                DayOfMonth = category.DayOfMonth,
                EstimatedAmount = category.EstimatedAmount
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var category = new Category

            {
                UserId = userId,

                Title = dto.Title,

                Icon = dto.Icon,

                Type = dto.Type,

                IsFixedExpense = dto.IsFixedExpense,

                DayOfMonth = dto.DayOfMonth,

                EstimatedAmount = dto.EstimatedAmount
            };

            context.Categories.Add(category);

            await context.SaveChangesAsync();

            dto.CategoryId = category.CategoryId;
            dto.UserId = userId;

            //  Generar notificación inmediata si es gasto fijo
            if (category.IsFixedExpense && category.DayOfMonth.HasValue)
            {
                try
                {
                    await _notificationService.GenerateNotificationForCategoryAsync(category.CategoryId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al generar notificación para categoría {category.CategoryId}: {ex.Message}");
                    // No fallar la creación de categoría por error en notificaciones
                }
            }

            return dto;
        }
        public async Task<bool> UpdateAsync(int id, CategoryDto dto, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories

                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)

                return false;

            category.Title = dto.Title;

            category.Icon = dto.Icon;

            category.Type = dto.Type;

            category.IsFixedExpense = dto.IsFixedExpense;

            category.DayOfMonth = dto.DayOfMonth;

            category.EstimatedAmount = dto.EstimatedAmount;

            // SINCRONIZACIÓN: Si esta categoría pertenece a un préstamo, actualizar el préstamo también

            var loan = await context.Loans

                .FirstOrDefaultAsync(l => l.CategoryId == id && l.UserId == userId);

            if (loan != null)

            {
                Console.WriteLine($"🔄 Categoría pertenece al préstamo '{loan.Title}'. Sincronizando cambios...");

                // Sincronizar campos editables

                loan.Title = dto.Title;

                loan.Icon = dto.Icon;

                // Si cambiaron el día de pago en la categoría, actualizarlo en el préstamo

                if (dto.DayOfMonth.HasValue)

                {
                    loan.DueDay = dto.DayOfMonth.Value;
                }

                // Si cambiaron el monto estimado en la categoría, actualizarlo en el préstamo

                if (dto.EstimatedAmount.HasValue)

                {
                    loan.InstallmentAmount = dto.EstimatedAmount.Value;
                }

                Console.WriteLine($"  ✅ Título: {loan.Title}");

                Console.WriteLine($"  ✅ Icono: {loan.Icon}");

                Console.WriteLine($"  ✅ Día de pago: {loan.DueDay}");

                Console.WriteLine($"  ✅ Cuota mensual: {loan.InstallmentAmount:C}");
            }

            await context.SaveChangesAsync();

            //  ACTUALIZAR NOTIFICACIONES: Si es gasto fijo, regenerar notificaciones
            if (category.IsFixedExpense && category.DayOfMonth.HasValue)
            {
                try
                {
                    // 1. Eliminar notificaciones futuras con la fecha vieja
                    await _notificationService.DeleteFutureNotificationsByCategoryAsync(category.CategoryId, userId);

                    // 2. Generar nueva notificación con la fecha actualizada
                    await _notificationService.GenerateNotificationForCategoryAsync(category.CategoryId);

                    Console.WriteLine($"✅ Notificaciones actualizadas para categoría '{category.Title}' con nueva fecha: día {category.DayOfMonth.Value}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al actualizar notificaciones para categoría {category.CategoryId}: {ex.Message}");
                    // No fallar la actualización por error en notificaciones
                }
            }
            else if (!category.IsFixedExpense)
            {
                // Si dejó de ser gasto fijo, eliminar todas las notificaciones futuras
                try
                {
                    await _notificationService.DeleteFutureNotificationsByCategoryAsync(category.CategoryId, userId);
                    Console.WriteLine($"🗑️ Notificaciones eliminadas - la categoría '{category.Title}' ya no es gasto fijo");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error al eliminar notificaciones: {ex.Message}");
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories

                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);

            if (category == null)

                return false;

            context.Categories.Remove(category);

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetRelatedTransactionsCountAsync(int categoryId, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpensesIncomes

                .CountAsync(ei => ei.CategoryId == categoryId && ei.UserId == userId);
        }

        public async Task<(bool BelongsToLoan, string? LoanTitle)> CheckIfBelongsToLoanAsync(int categoryId, string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var loan = await context.Loans

                .FirstOrDefaultAsync(l => l.CategoryId == categoryId && l.UserId == userId);

            if (loan != null)

            {
                return (true, loan.Title);
            }

            return (false, null);
        }

        public async Task<bool> ExistsCategoryWithNameAsync(string title, TransactionType type, string userId, int? excludeCategoryId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Categories

                .Where(c => c.UserId == userId

                    && c.Type == type

                    && c.Title.ToLower() == title.ToLower());

            if (excludeCategoryId.HasValue)

            {
                query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsCategoryWithIconAsync(string icon, TransactionType type, string userId, int? excludeCategoryId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Categories
                .Where(c => c.UserId == userId
                    && c.Type == type
                    && c.Icon == icon);

            if (excludeCategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId != excludeCategoryId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
