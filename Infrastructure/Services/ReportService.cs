using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services

{

    public class ReportService : IReportService

    {

        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReportService(IDbContextFactory<ApplicationDbContext> contextFactory)

        {

            _contextFactory = contextFactory;

        }

        // Genera los datos completos del reporte según los filtros

        public async Task<ReportDataDto> GenerateReportDataAsync(ReportFilterDto filter)

        {

            // 1. Calcular fechas del período

            var (startDate, endDate) = CalculateDateRange(filter);

            // 2. Obtener nombre del usuario del filtro

            var userName = string.IsNullOrEmpty(filter.UserName) ? "Usuario" : filter.UserName;

            // 3. Obtener transacciones filtradas

            var transactions = await GetFilteredTransactionsAsync(filter, startDate, endDate);

            // 4. Calcular resumen

            var summary = CalculateSummary(transactions, startDate, endDate);

            // 5. Calcular comparativa con período anterior

            var comparison = await CalculateComparisonAsync(filter, startDate, endDate, summary);

            // 6. Agrupar gastos por categoría

            var expensesByCategory = CalculateExpensesByCategory(transactions);

            // 7. Agrupar ingresos por categoría

            var incomesByCategory = CalculateIncomesByCategory(transactions);

            // 8. Construir el reporte

            var report = new ReportDataDto

            {

                UserName = userName,

                StartDate = startDate,

                EndDate = endDate,

                GeneratedAt = DateTime.Now,

                PeriodDescription = GetPeriodDescription(filter, startDate, endDate),

                Summary = summary,

                Comparison = comparison,

                ExpensesByCategory = expensesByCategory,

                IncomesByCategory = incomesByCategory,

                Transactions = transactions

            };

            return report;

        }

        #region Private Helper Methods

        // Calcula el rango de fechas según el tipo de período

        private (DateTime startDate, DateTime endDate) CalculateDateRange(ReportFilterDto filter)

        {

            var today = DateTime.Now.Date;

            DateTime startDate, endDate;

            switch (filter.PeriodType)

            {

                case ReportPeriodType.LastWeek:

                    startDate = today.AddDays(-7);

                    endDate = today.AddDays(1).AddTicks(-1); // Fin del día de hoy

                    break;

                case ReportPeriodType.LastMonth:

                    startDate = today.AddMonths(-1);

                    endDate = today.AddDays(1).AddTicks(-1);

                    break;

                case ReportPeriodType.Last3Months:

                    startDate = today.AddMonths(-3);

                    endDate = today.AddDays(1).AddTicks(-1);

                    break;

                case ReportPeriodType.Last6Months:

                    startDate = today.AddMonths(-6);

                    endDate = today.AddDays(1).AddTicks(-1);

                    break;

                case ReportPeriodType.LastYear:

                    startDate = today.AddYears(-1);

                    endDate = today.AddDays(1).AddTicks(-1);

                    break;

                case ReportPeriodType.Custom:

                    startDate = (filter.CustomStartDate ?? today.AddMonths(-1)).Date;

                    endDate = (filter.CustomEndDate ?? today).Date.AddDays(1).AddTicks(-1);

                    break;

                default:

                    startDate = today.AddMonths(-1);

                    endDate = today.AddDays(1).AddTicks(-1);

                    break;

            }

            return (startDate, endDate);

        }

        // Obtiene transacciones filtradas según los criterios

        private async Task<List<ExpenseIncomeDto>> GetFilteredTransactionsAsync(

            ReportFilterDto filter, DateTime startDate, DateTime endDate)

        {

            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.ExpensesIncomes

                .Include(ei => ei.Category)

                .Where(ei => ei.UserId == filter.UserId

                    && ei.Date >= startDate

                    && ei.Date <= endDate);

            // Filtrar por tipo de transacción

            if (filter.TransactionTypeFilter.HasValue)

            {

                query = query.Where(ei => ei.Type == filter.TransactionTypeFilter.Value);

            }

            // Filtrar por categoría

            if (filter.CategoryIdFilter.HasValue && filter.CategoryIdFilter.Value > 0)

            {

                query = query.Where(ei => ei.CategoryId == filter.CategoryIdFilter.Value);

            }

            var transactions = await query

                .OrderByDescending(ei => ei.Date)

                .Select(ei => new ExpenseIncomeDto

                {

                    Id = ei.Id,

                    CategoryId = ei.CategoryId,

                    CategoryTitle = ei.Category!.Title,

                    CategoryIcon = ei.Category.Icon,

                    Amount = ei.Amount,

                    Date = ei.Date,

                    Description = ei.Description,

                    Type = ei.Type,

                    UserId = ei.UserId

                })

                .ToListAsync();

            return transactions;

        }

        // Calcula el resumen estadístico

        private ReportSummaryDto CalculateSummary(List<ExpenseIncomeDto> transactions, DateTime startDate, DateTime endDate)

        {

            var totalIncome = transactions

                .Where(t => t.Type == TransactionType.Income)

                .Sum(t => t.Amount);

            var totalExpense = transactions

                .Where(t => t.Type == TransactionType.Expense)

                .Sum(t => t.Amount);

            var totalDays = (endDate - startDate).Days + 1;

            var averageDailyExpense = totalDays > 0 ? totalExpense / totalDays : 0;

            return new ReportSummaryDto

            {

                TotalIncome = totalIncome,

                TotalExpense = totalExpense,

                Balance = totalIncome - totalExpense,

                AverageDailyExpense = averageDailyExpense,

                TotalTransactions = transactions.Count,

                TotalIncomeTransactions = transactions.Count(t => t.Type == TransactionType.Income),

                TotalExpenseTransactions = transactions.Count(t => t.Type == TransactionType.Expense)

            };

        }

        // Calcula la comparativa con el período anterior

        private async Task<ReportComparisonDto?> CalculateComparisonAsync(

            ReportFilterDto filter, DateTime startDate, DateTime endDate, ReportSummaryDto currentSummary)

        {

            try

            {

                using var context = await _contextFactory.CreateDbContextAsync();

                // Calcular el período anterior (mismo rango de días)

                var daysDifference = (endDate - startDate).Days;

                var previousStartDate = startDate.AddDays(-(daysDifference + 1));

                var previousEndDate = startDate.AddDays(-1);

                // Obtener transacciones del período anterior

                var previousTransactions = await context.ExpensesIncomes

                    .Where(ei => ei.UserId == filter.UserId

                        && ei.Date >= previousStartDate

                        && ei.Date <= previousEndDate)

                    .ToListAsync();

                var previousIncome = previousTransactions

                    .Where(t => t.Type == TransactionType.Income)

                    .Sum(t => t.Amount);

                var previousExpense = previousTransactions

                    .Where(t => t.Type == TransactionType.Expense)

                    .Sum(t => t.Amount);

                var previousBalance = previousIncome - previousExpense;

                // Calcular cambios porcentuales

                var incomeChange = CalculatePercentageChange(previousIncome, currentSummary.TotalIncome);

                var expenseChange = CalculatePercentageChange(previousExpense, currentSummary.TotalExpense);

                var balanceChange = CalculatePercentageChange(previousBalance, currentSummary.Balance);

                return new ReportComparisonDto

                {

                    IncomeChange = incomeChange,

                    ExpenseChange = expenseChange,

                    BalanceChange = balanceChange,

                    IncomeChangeDisplay = FormatPercentageChange(incomeChange),

                    ExpenseChangeDisplay = FormatPercentageChange(expenseChange),

                    BalanceChangeDisplay = FormatPercentageChange(balanceChange)

                };

            }

            catch

            {
                // Si hay error en la comparación, devolver null

                return null;
            }

        }

        // Agrupa gastos por categoría

        private List<ReportCategoryExpenseDto> CalculateExpensesByCategory(List<ExpenseIncomeDto> transactions)

        {

            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

            var totalExpenses = expenses.Sum(e => e.Amount);

            var grouped = expenses

                .GroupBy(e => new { e.CategoryId, e.CategoryTitle, e.CategoryIcon })

                .Select(g => new ReportCategoryExpenseDto

                {

                    CategoryName = g.Key.CategoryTitle ?? "Sin categoría",

                    CategoryIcon = g.Key.CategoryIcon ?? "📁",

                    TotalAmount = g.Sum(e => e.Amount),

                    Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount) / totalExpenses) * 100 : 0,

                    TransactionCount = g.Count()

                })

                .OrderByDescending(c => c.TotalAmount)

                .ToList();

            return grouped;

        }

        // Agrupa ingresos por categoría

        private List<ReportCategoryIncomeDto> CalculateIncomesByCategory(List<ExpenseIncomeDto> transactions)

        {

            var incomes = transactions.Where(t => t.Type == TransactionType.Income).ToList();

            var totalIncomes = incomes.Sum(i => i.Amount);



            var grouped = incomes

                .GroupBy(i => new { i.CategoryId, i.CategoryTitle, i.CategoryIcon })

                .Select(g => new ReportCategoryIncomeDto

                {

                    CategoryName = g.Key.CategoryTitle ?? "Sin categoría",

                    CategoryIcon = g.Key.CategoryIcon ?? "📁",

                    TotalAmount = g.Sum(i => i.Amount),

                    Percentage = totalIncomes > 0 ? (g.Sum(i => i.Amount) / totalIncomes) * 100 : 0,

                    TransactionCount = g.Count()

                })

                .OrderByDescending(c => c.TotalAmount)

                .ToList();



            return grouped;

        }

        // Calcula el cambio porcentual entre dos valores

        private decimal CalculatePercentageChange(decimal oldValue, decimal newValue)

        {

            if (oldValue == 0)

            {

                return newValue > 0 ? 100 : 0;

            }



            return ((newValue - oldValue) / oldValue) * 100;

        }

        // Formatea el cambio porcentual con flecha

        private string FormatPercentageChange(decimal change)

        {

            var arrow = change >= 0 ? "↑" : "↓";

            return $"{arrow} {Math.Abs(change):F1}%";

        }

        // Genera descripción del período

        private string GetPeriodDescription(ReportFilterDto filter, DateTime startDate, DateTime endDate)

        {

            return filter.PeriodType switch

            {

                ReportPeriodType.LastWeek => "Última semana",

                ReportPeriodType.LastMonth => "Último mes",

                ReportPeriodType.Last3Months => "Últimos 3 meses",

                ReportPeriodType.Last6Months => "Últimos 6 meses",

                ReportPeriodType.LastYear => "Último año",

                ReportPeriodType.Custom => $"Del {startDate:dd/MM/yyyy} al {endDate:dd/MM/yyyy}",

                _ => "Período personalizado"

            };
        }
        #endregion
    }
}