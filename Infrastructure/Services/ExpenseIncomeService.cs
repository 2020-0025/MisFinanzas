using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services

{
    public class ExpenseIncomeService : IExpenseIncomeService

    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ExpenseIncomeService(IDbContextFactory<ApplicationDbContext> contextFactory)

        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ExpenseIncomeDto>> GetAllByUserAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpensesIncomes

                .Include(gi => gi.Category)

                .Where(gi => gi.UserId == userId)

                .OrderByDescending(gi => gi.Date)

                .Select(gi => new ExpenseIncomeDto

                {
                    Id = gi.Id,

                    UserId = gi.UserId,

                    CategoryId = gi.CategoryId,

                    CategoryTitle = gi.Category.Title,

                    CategoryIcon = gi.Category.Icon,

                    Amount = gi.Amount,

                    Date = gi.Date,

                    Description = gi.Description,

                    Type = gi.Type

                })

                .ToListAsync();
        }

        public async Task<List<ExpenseIncomeDto>> GetByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpensesIncomes

                .Include(gi => gi.Category)

                .Where(gi => gi.UserId == userId && gi.Date >= startDate && gi.Date <= endDate)

                .OrderByDescending(gi => gi.Date)

                .Select(gi => new ExpenseIncomeDto

                {
                    Id = gi.Id,

                    UserId = gi.UserId,

                    CategoryId = gi.CategoryId,

                    CategoryTitle = gi.Category.Title,

                    CategoryIcon = gi.Category.Icon,

                    Amount = gi.Amount,

                    Date = gi.Date,

                    Description = gi.Description,

                    Type = gi.Type

                })

                .ToListAsync();
        }
        public async Task<List<ExpenseIncomeDto>> GetByUserAndTypeAsync(string userId, TransactionType type)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpensesIncomes

                .Include(gi => gi.Category)

                .Where(gi => gi.UserId == userId && gi.Type == type)

                .OrderByDescending(gi => gi.Date)

                .Select(gi => new ExpenseIncomeDto

                {
                    Id = gi.Id,

                    UserId = gi.UserId,

                    CategoryId = gi.CategoryId,

                    CategoryTitle = gi.Category.Title,

                    CategoryIcon = gi.Category.Icon,

                    Amount = gi.Amount,

                    Date = gi.Date,

                    Description = gi.Description,

                    Type = gi.Type
                })

                .ToListAsync();
        }

        public async Task<ExpenseIncomeDto?> GetByIdAsync(int id, string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var gastoIngreso = await context.ExpensesIncomes

                .Include(gi => gi.Category)

                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)

                return null;

            return new ExpenseIncomeDto

            {
                Id = gastoIngreso.Id,

                UserId = gastoIngreso.UserId,

                CategoryId = gastoIngreso.CategoryId,

                CategoryTitle = gastoIngreso.Category.Title,

                CategoryIcon = gastoIngreso.Category.Icon,

                Amount = gastoIngreso.Amount,

                Date = gastoIngreso.Date,

                Description = gastoIngreso.Description,

                Type = gastoIngreso.Type
            };
        }

        public async Task<(bool Success, string? Error, ExpenseIncomeDto? Data)> CreateAsync(ExpenseIncomeDto dto, string userId)

        {
            try

            {
                // VALIDACIÓN: Si es un GASTO, verificar que haya balance suficiente

                if (dto.Type == TransactionType.Expense)

                {
                    var currentBalance = await GetBalanceByUserAsync(userId);

                    if (currentBalance <= 0)

                    {
                        return (false, "No tienes saldo disponible para registrar gastos.", null);
                    }

                    if (dto.Amount > currentBalance)

                    {
                        return (false, $"Saldo insuficiente. Tu balance actual es {currentBalance:C} y el gasto es {dto.Amount:C}.", null);
                    }
                }

                using var context = await _contextFactory.CreateDbContextAsync();

                var ExpenseIncome = new ExpenseIncome

                {
                    UserId = userId,

                    CategoryId = dto.CategoryId,

                    Amount = dto.Amount,

                    Date = dto.Date,

                    Description = dto.Description,

                    Type = dto.Type,

                    CreatedAt = DateTime.UtcNow
                };

                context.ExpensesIncomes.Add(ExpenseIncome);

                await context.SaveChangesAsync();

                dto.Id = ExpenseIncome.Id;

                dto.UserId = userId;

                Console.WriteLine($"✅ {(dto.Type == TransactionType.Income ? "Ingreso" : "Gasto")} registrado: {dto.Amount:C}");

                return (true, null, dto);
            }

            catch (Exception ex)

            {
                Console.WriteLine($"❌ Error al crear transacción: {ex.Message}");

                return (false, "Error al registrar la transacción.", null);
            }
        }

        public async Task<bool> UpdateAsync(int id, ExpenseIncomeDto dto, string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var gastoIngreso = await context.ExpensesIncomes

                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)

                return false;

            gastoIngreso.CategoryId = dto.CategoryId;

            gastoIngreso.Amount = dto.Amount;

            gastoIngreso.Date = dto.Date;

            gastoIngreso.Description = dto.Description;

            gastoIngreso.Type = dto.Type;

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var gastoIngreso = await context.ExpensesIncomes

                .FirstOrDefaultAsync(gi => gi.Id == id && gi.UserId == userId);

            if (gastoIngreso == null)

                return false;

            // VERIFICAR: ¿Este ExpenseIncome pertenece a un préstamo?

            var loan = await context.Loans

                .FirstOrDefaultAsync(l => l.CategoryId == gastoIngreso.CategoryId && l.UserId == userId);

            if (loan != null && gastoIngreso.Type == TransactionType.Expense)

            {
                // Este pago pertenece a un préstamo - ajustar contador

                Console.WriteLine($"⚠️ Eliminando pago de préstamo '{loan.Title}'. Ajustando contador...");

                // Validar que el contador no quede negativo

                if (loan.InstallmentsPaid > 0)

                {
                    loan.InstallmentsPaid--;

                    Console.WriteLine($"✅ Contador ajustado: {loan.InstallmentsPaid + 1} → {loan.InstallmentsPaid}");
                }

                // Si el préstamo estaba completado, reactivarlo

                if (!loan.IsActive && loan.InstallmentsPaid < loan.NumberOfInstallments)

                {
                    loan.IsActive = true;

                    Console.WriteLine($"✅ Préstamo reactivado (aún faltan {loan.NumberOfInstallments - loan.InstallmentsPaid} cuotas)");
                }
            }

            context.ExpensesIncomes.Remove(gastoIngreso);

            await context.SaveChangesAsync();

            return true;
        }

        // Cálculos

        public async Task<decimal> GetTotalIngresosByUserAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.ExpensesIncomes

                .Where(ei => ei.UserId == userId && ei.Type == TransactionType.Income)

                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        public async Task<decimal> GetTotalGastosByUserAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.ExpensesIncomes

                .Where(ei => ei.UserId == userId && ei.Type == TransactionType.Expense)

                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        public async Task<decimal> GetBalanceByUserAsync(string userId)

        {
            var ingresos = await GetTotalIngresosByUserAsync(userId);

            var gastos = await GetTotalGastosByUserAsync(userId);

            return ingresos - gastos;
        }
        public async Task<decimal> GetIngresosMesActualAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            var items = await context.ExpensesIncomes

                .Where(ei => ei.UserId == userId &&

                            ei.Type == TransactionType.Income &&

                            ei.Date >= primerDiaMes &&

                            ei.Date <= ultimoDiaMes)

                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        public async Task<decimal> GetGastosMesActualAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            var items = await context.ExpensesIncomes

                .Where(ei => ei.UserId == userId &&

                            ei.Type == TransactionType.Expense &&

                            ei.Date >= primerDiaMes &&

                            ei.Date <= ultimoDiaMes)

                .ToListAsync();

            return items.Sum(ei => ei.Amount);
        }

        // ========== MÉTODOS PARA DASHBOARD ==========
        public async Task<Dictionary<DateTime, decimal>> GetDailyIncomesAsync(string userId, DateTime startDate, DateTime endDate)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.ExpensesIncomes

                .Where(e => e.UserId == userId &&

                            e.Type == TransactionType.Income &&

                            e.Date >= startDate &&

                            e.Date <= endDate)

                .ToListAsync();

            return items

                .GroupBy(e => e.Date.Date)

                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyExpensesAsync(string userId, DateTime startDate, DateTime endDate)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var items = await context.ExpensesIncomes

                .Where(e => e.UserId == userId &&

                            e.Type == TransactionType.Expense &&

                            e.Date >= startDate &&

                            e.Date <= endDate)

                .ToListAsync();

            return items

                .GroupBy(e => e.Date.Date)

                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<List<ExpenseIncomeDto>> GetRecentTransactionsAsync(string userId, int count)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpensesIncomes

                .Include(e => e.Category)

                .Where(e => e.UserId == userId)

                .OrderByDescending(e => e.Date)

                .ThenByDescending(e => e.Id)

                .Take(count)

                .Select(e => new ExpenseIncomeDto

                {
                    Id = e.Id,

                    UserId = e.UserId,

                    CategoryId = e.CategoryId,

                    CategoryTitle = e.Category.Title,

                    CategoryIcon = e.Category.Icon,

                    Amount = e.Amount,

                    Date = e.Date,

                    Description = e.Description,

                    Type = e.Type

                })

                .ToListAsync();
        }
        public async Task<(decimal Ingresos, decimal Gastos)> GetTotalsByMonthAsync(string userId, int month, int year)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var firstDay = new DateTime(year, month, 1);

            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var items = await context.ExpensesIncomes

                .Where(e => e.UserId == userId &&

                            e.Date >= firstDay &&

                            e.Date <= lastDay)

                .ToListAsync();

            var ingresos = items.Where(e => e.Type == TransactionType.Income).Sum(e => e.Amount);

            var gastos = items.Where(e => e.Type == TransactionType.Expense).Sum(e => e.Amount);

            return (ingresos, gastos);
        }
    }
}