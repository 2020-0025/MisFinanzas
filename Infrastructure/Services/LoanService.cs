using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services

{
    public class LoanService : ILoanService

    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        private readonly INotificationService _notificationService;

        public LoanService(IDbContextFactory<ApplicationDbContext> contextFactory, INotificationService notificationService)

        {
            _contextFactory = contextFactory;

            _notificationService = notificationService;
        }

        // ========== MÉTODOS DE MAPEO ==========

        private LoanDto MapToDto(Loan loan)
        {
            return new LoanDto
            {
                LoanId = loan.LoanId,
                Title = loan.Title,
                Description = loan.Description,
                PrincipalAmount = loan.PrincipalAmount,
                InstallmentAmount = loan.InstallmentAmount,
                NumberOfInstallments = loan.NumberOfInstallments,
                DueDay = loan.DueDay,
                StartDate = loan.StartDate,
                Icon = loan.Icon,
                IsActive = loan.IsActive,
                InstallmentsPaid = loan.InstallmentsPaid,
                UserId = loan.UserId,
                CategoryId = loan.CategoryId,
                InterestRate = loan.InterestRate, //  NUEVO
                Installments = loan.Installments?.Select(i => new LoanInstallmentDto //  NUEVO
                {
                    Id = i.Id,
                    LoanId = i.LoanId,
                    InstallmentNumber = i.InstallmentNumber,
                    DueDate = i.DueDate,
                    PrincipalAmount = i.PrincipalAmount,
                    InterestAmount = i.InterestAmount,
                    TotalAmount = i.TotalAmount,
                    RemainingBalance = i.RemainingBalance,
                    IsPaid = i.IsPaid,
                    PaidDate = i.PaidDate,
                    ExpenseIncomeId = i.ExpenseIncomeId
                }).ToList() ?? new List<LoanInstallmentDto>()
            };
        }


        private Loan MapToEntity(LoanDto dto)

        {
            return new Loan

            {
                LoanId = dto.LoanId,

                Title = dto.Title,

                Description = dto.Description,

                PrincipalAmount = dto.PrincipalAmount,

                InstallmentAmount = dto.InstallmentAmount,

                NumberOfInstallments = dto.NumberOfInstallments,

                DueDay = dto.DueDay,

                StartDate = dto.StartDate,

                Icon = dto.Icon,

                IsActive = dto.IsActive,

                InstallmentsPaid = dto.InstallmentsPaid,

                UserId = dto.UserId,

                CategoryId = dto.CategoryId

            };
        }

        // ========== CONSULTAS BÁSICAS ==========

        public async Task<List<LoanDto>> GetAllByUserAsync(string userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var loans = await context.Loans
                .Include(l => l.Category)
                .Include(l => l.Installments) //  CARGAR CUOTAS
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();

            return loans.Select(MapToDto).ToList();
        }


        public async Task<List<LoanDto>> GetActiveByUserAsync(string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var loans = await context.Loans

                .Include(l => l.Category)
                .Include(l => l.Installments)

                .Where(l => l.UserId == userId && l.IsActive)

                .OrderByDescending(l => l.StartDate)

                .ToListAsync();

            return loans.Select(MapToDto).ToList();

        }

        public async Task<LoanDto?> GetByIdAsync(int loanId, string userId)

        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var loan = await context.Loans

                .Include(l => l.Category)
                .Include(l => l.Installments)

                .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

            return loan != null ? MapToDto(loan) : null;

        }
        // ========== CRUD ==========

        public async Task<(bool Success, string? Error, LoanDto? Loan)> CreateAsync(LoanDto loanDto, string userId, bool createReminder = false)
        {
            try
            {
                // Validar que no exista préstamo con el mismo título
                if (await ExistsLoanWithTitleAsync(loanDto.Title, userId))
                {
                    return (false, "Ya existe un préstamo con ese título.", null);
                }

                // Validar datos
                if (loanDto.PrincipalAmount <= 0)
                    return (false, "El monto del préstamo debe ser mayor a cero.", null);

                if (loanDto.InstallmentAmount <= 0)
                    return (false, "La cuota mensual debe ser mayor a cero.", null);

                if (loanDto.NumberOfInstallments < 1)
                    return (false, "El número de cuotas debe ser al menos 1.", null);

                if (loanDto.DueDay < 1 || loanDto.DueDay > 31)
                    return (false, "El día de pago debe estar entre 1 y 31.", null);

                using var context = await _contextFactory.CreateDbContextAsync();

                // ⭐ CALCULAR O USAR TASA DE INTERÉS
                decimal interestRate = loanDto.InterestRate ??
                    LoanAmortizationHelper.CalculateApproximateInterestRate(
                        loanDto.PrincipalAmount,
                        loanDto.InstallmentAmount,
                        loanDto.NumberOfInstallments);

                // 1. Crear categoría automáticamente para este préstamo
                var category = new Category
                {
                    UserId = userId,
                    Title = loanDto.Title,
                    Icon = loanDto.Icon,
                    Type = TransactionType.Expense,
                    IsFixedExpense = createReminder,
                    DayOfMonth = createReminder ? loanDto.DueDay : null,
                    EstimatedAmount = createReminder ? loanDto.InstallmentAmount : null
                };

                context.Categories.Add(category);
                await context.SaveChangesAsync();

                // 2. Crear préstamo
                var loan = new Loan
                {
                    UserId = userId,
                    Title = loanDto.Title,
                    Description = loanDto.Description,
                    PrincipalAmount = loanDto.PrincipalAmount,
                    InstallmentAmount = loanDto.InstallmentAmount,
                    NumberOfInstallments = loanDto.NumberOfInstallments,
                    DueDay = loanDto.DueDay,
                    StartDate = loanDto.StartDate,
                    Icon = loanDto.Icon,
                    IsActive = true,
                    InstallmentsPaid = 0,
                    CategoryId = category.CategoryId,
                    InterestRate = interestRate // ⭐ GUARDAR TASA
                };

                context.Loans.Add(loan);
                await context.SaveChangesAsync();

                // ⭐ 3. GENERAR TABLA DE AMORTIZACIÓN
                var amortizationSchedule = LoanAmortizationHelper.GenerateAmortizationSchedule(
                    loanDto.PrincipalAmount,
                    interestRate,
                    loanDto.NumberOfInstallments,
                    loanDto.StartDate,
                    loanDto.DueDay);

                foreach (var (number, dueDate, principal, interest, total, balance) in amortizationSchedule)
                {
                    var installment = new LoanInstallment
                    {
                        LoanId = loan.LoanId,
                        InstallmentNumber = number,
                        DueDate = dueDate,
                        PrincipalAmount = principal,
                        InterestAmount = interest,
                        TotalAmount = total,
                        RemainingBalance = balance,
                        IsPaid = false
                    };

                    context.LoanInstallments.Add(installment);
                }

                await context.SaveChangesAsync();

                // 4. Registrar INGRESO inicial del préstamo
                var initialIncome = new ExpenseIncome
                {
                    UserId = userId,
                    CategoryId = category.CategoryId,
                    Type = TransactionType.Income,
                    Amount = loanDto.PrincipalAmount,
                    Description = $"Préstamo recibido: {loanDto.Title}",
                    Date = loanDto.StartDate
                };

                context.ExpensesIncomes.Add(initialIncome);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Préstamo '{loan.Title}' creado con {amortizationSchedule.Count} cuotas. Tasa: {interestRate}%");

                return (true, null, MapToDto(loan));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al crear préstamo: {ex.Message}");
                return (false, ex.Message, null);
            }
        }


        public async Task<bool> UpdateAsync(int loanId, LoanDto updatedLoanDto, string userId)

        {
            try

            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans

                    .Include(l => l.Category)

                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)

                    return false;

                // Validar que no exista otro préstamo con el mismo título

                if (await ExistsLoanWithTitleAsync(updatedLoanDto.Title, userId, loanId))

                    return false;

                // Actualizar datos del préstamo

                loan.Title = updatedLoanDto.Title;

                loan.Description = updatedLoanDto.Description;

                loan.PrincipalAmount = updatedLoanDto.PrincipalAmount;

                loan.InstallmentAmount = updatedLoanDto.InstallmentAmount;

                loan.NumberOfInstallments = updatedLoanDto.NumberOfInstallments;

                loan.DueDay = updatedLoanDto.DueDay;

                loan.StartDate = updatedLoanDto.StartDate;

                loan.Icon = updatedLoanDto.Icon;

                // Actualizar categoría asociada

                if (loan.Category != null)

                {
                    loan.Category.Title = updatedLoanDto.Title;

                    loan.Category.Icon = updatedLoanDto.Icon;

                    loan.Category.EstimatedAmount = updatedLoanDto.InstallmentAmount;
                }

                await context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)

            {
                Console.WriteLine($"❌ Error al actualizar préstamo: {ex.Message}");

                return false;
            }
        }

        public async Task<bool> DeleteAsync(int loanId, string userId, bool deleteHistory = false)

        {
            try

            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans

                    .Include(l => l.Category)

                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)

                    return false;

                if (deleteHistory)

                {
                    Console.WriteLine($"🗑️ Eliminando préstamo '{loan.Title}' con TODO el historial...");

                    // 1. Eliminar TODAS las notificaciones relacionadas con esta categoría

                    var relatedNotifications = await context.Notifications

                        .Where(n => n.CategoryId == loan.CategoryId && n.UserId == userId)

                        .ToListAsync();

                    if (relatedNotifications.Any())

                    {

                        context.Notifications.RemoveRange(relatedNotifications);

                        Console.WriteLine($"  ✅ {relatedNotifications.Count} notificación(es) eliminada(s)");

                    }

                    // 2. Eliminar TODOS los ExpenseIncomes (pagos) relacionados con esta categoría

                    var relatedExpenses = await context.ExpensesIncomes

                        .Where(e => e.CategoryId == loan.CategoryId && e.UserId == userId)

                        .ToListAsync();

                    if (relatedExpenses.Any())

                    {
                        context.ExpensesIncomes.RemoveRange(relatedExpenses);

                        Console.WriteLine($"  ✅ {relatedExpenses.Count} pago(s) eliminado(s)");
                    }

                    // 3. Eliminar el PRÉSTAMO (ANTES de la categoría por restricción FK)

                    context.Loans.Remove(loan);

                    Console.WriteLine($"  ✅ Préstamo eliminado");

                    // 4. Eliminar la CATEGORÍA (DESPUÉS del préstamo)

                    if (loan.Category != null)

                    {
                        context.Categories.Remove(loan.Category);

                        Console.WriteLine($"  ✅ Categoría '{loan.Category.Title}' eliminada");
                    }
                }

                else

                {
                    // Opción B: Marcar como inactivo (preservar historial)

                    Console.WriteLine($"📦 Archivando préstamo '{loan.Title}' (preservando historial)...");

                    loan.IsActive = false;
                }

                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Operación completada exitosamente");

                return true;
            }

            catch (Exception ex)

            {
                Console.WriteLine($"❌ Error al eliminar préstamo: {ex.Message}");

                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                return false;
            }
        }
        // ========== OPERACIONES ESPECÍFICAS DE PRÉSTAMOS ==========
        public async Task<bool> RegisterPaymentAsync(int loanId, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans
                    .Include(l => l.Installments)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null || !loan.IsActive)
                    return false;

                // Validar que no haya pagado todas las cuotas
                if (loan.InstallmentsPaid >= loan.NumberOfInstallments)
                    return false;

                // ⭐ BUSCAR SIGUIENTE CUOTA PENDIENTE
                var nextInstallment = loan.Installments
                    .Where(i => !i.IsPaid)
                    .OrderBy(i => i.InstallmentNumber)
                    .FirstOrDefault();

                if (nextInstallment == null)
                    return false;

                // ⭐ REGISTRAR SOLO EL INTERÉS COMO GASTO
                var interestExpense = new ExpenseIncome
                {
                    UserId = userId,
                    CategoryId = loan.CategoryId,
                    Type = TransactionType.Expense,
                    Amount = nextInstallment.InterestAmount, //  SOLO INTERÉS
                    Description = $"Interés cuota {nextInstallment.InstallmentNumber}/{loan.NumberOfInstallments} - {loan.Title}",
                    Date = DateTime.Now
                };

                context.ExpensesIncomes.Add(interestExpense);
                await context.SaveChangesAsync();

                //  MARCAR CUOTA COMO PAGADA
                nextInstallment.IsPaid = true;
                nextInstallment.PaidDate = DateTime.Now;
                nextInstallment.ExpenseIncomeId = interestExpense.Id;

                // Incrementar contador
                loan.InstallmentsPaid++;

                // Si completó todas las cuotas, marcar préstamo como completado
                if (loan.InstallmentsPaid >= loan.NumberOfInstallments)
                {
                    loan.IsActive = false;
                }

                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Cuota {nextInstallment.InstallmentNumber} pagada. Capital: {nextInstallment.PrincipalAmount:C}, Interés: {nextInstallment.InterestAmount:C}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al registrar pago: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UndoLastPaymentAsync(int loanId, string userId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans
                    .Include(l => l.Installments)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)
                    return false;

                // Validar que haya al menos un pago registrado
                if (loan.InstallmentsPaid <= 0)
                    return false;

                //  BUSCAR ÚLTIMA CUOTA PAGADA
                var lastPaidInstallment = loan.Installments
                    .Where(i => i.IsPaid)
                    .OrderByDescending(i => i.InstallmentNumber)
                    .FirstOrDefault();

                if (lastPaidInstallment == null)
                    return false;

                //  ELIMINAR EL GASTO DEL INTERÉS
                if (lastPaidInstallment.ExpenseIncomeId.HasValue)
                {
                    var expense = await context.ExpensesIncomes
                        .FirstOrDefaultAsync(ei => ei.Id == lastPaidInstallment.ExpenseIncomeId.Value);

                    if (expense != null)
                    {
                        context.ExpensesIncomes.Remove(expense);
                    }
                }

                //  MARCAR CUOTA COMO NO PAGADA
                lastPaidInstallment.IsPaid = false;
                lastPaidInstallment.PaidDate = null;
                lastPaidInstallment.ExpenseIncomeId = null;

                // Decrementar contador
                loan.InstallmentsPaid--;

                // Reactivar préstamo si estaba completado
                if (!loan.IsActive && loan.InstallmentsPaid < loan.NumberOfInstallments)
                {
                    loan.IsActive = true;
                }

                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Pago de cuota {lastPaidInstallment.InstallmentNumber} deshecho");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al deshacer pago: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> MarkAsCompletedAsync(int loanId, string userId)

        {
            try

            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans

                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)

                    return false;

                loan.IsActive = false;

                await context.SaveChangesAsync();

                return true;
            }

            catch (Exception ex)

            {
                Console.WriteLine($"❌ Error al marcar como completado: {ex.Message}");

                return false;
            }
        }

        public async Task<bool> ReactivateLoanAsync(int loanId, string userId)

        {
            try

            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var loan = await context.Loans

                    .FirstOrDefaultAsync(l => l.LoanId == loanId && l.UserId == userId);

                if (loan == null)

                    return false;

                // Solo se pueden reactivar préstamos cancelados (no completados)

                if (loan.IsActive)

                {
                    Console.WriteLine($"⚠️ El préstamo '{loan.Title}' ya está activo");

                    return false;
                }

                if (loan.InstallmentsPaid >= loan.NumberOfInstallments)

                {
                    Console.WriteLine($"⚠️ El préstamo '{loan.Title}' está completado. No se puede reactivar.");

                    return false;
                }

                // Reactivar el préstamo

                loan.IsActive = true;

                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Préstamo '{loan.Title}' reactivado. Cuotas pagadas: {loan.InstallmentsPaid}/{loan.NumberOfInstallments}");

                return true;
            }

            catch (Exception ex)

            {
                Console.WriteLine($"❌ Error al reactivar préstamo: {ex.Message}");

                return false;
            }

        }

        // ========== VALIDACIONES ==========

        public async Task<bool> ExistsLoanWithTitleAsync(string title, string userId, int? excludeLoanId = null)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Loans.Where(l => l.UserId == userId && l.Title == title);

            if (excludeLoanId.HasValue)

            {

                query = query.Where(l => l.LoanId != excludeLoanId.Value);

            }

            return await query.AnyAsync();

        }

        // ========== RESÚMENES Y ESTADÍSTICAS ==========

        public async Task<decimal> GetTotalBorrowedAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();



            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();



            return activeLoans.Sum(l => l.PrincipalAmount);

        }
        public async Task<decimal> GetTotalToPayAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            return activeLoans.Sum(l => l.TotalToPay);

        }

        public async Task<decimal> GetTotalPaidAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            return activeLoans.Sum(l => l.TotalPaid);

        }

        public async Task<decimal> GetTotalRemainingAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            return activeLoans.Sum(l => (l.TotalToPay - l.TotalPaid));

        }

        public async Task<decimal> GetMonthlyPaymentsTotalAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            return activeLoans.Sum(l => l.InstallmentAmount);

        }

        public async Task<decimal> GetAverageInterestRateAsync(string userId)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            if (!activeLoans.Any())

                return 0;

            return activeLoans.Average(l => l.ApproximateInterestRate);

        }

        // ========== PARA DASHBOARD ==========
        public async Task<List<LoanDto>> GetLoansWithUpcomingPaymentsAsync(string userId, int daysAhead = 7)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var activeLoans = await context.Loans

                .Where(l => l.UserId == userId && l.IsActive)

                .ToListAsync();

            var today = DateTime.Now;

            var futureDate = today.AddDays(daysAhead);

            var filtered = activeLoans

                .Where(l => l.NextPaymentDate >= today && l.NextPaymentDate <= futureDate)

                .OrderBy(l => l.NextPaymentDate)

                .ToList();

            return filtered.Select(MapToDto).ToList();

        }

    }

}