using MisFinanzas.Domain.DTOs;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services;

/// <summary>
/// Servicio que ejecuta comandos en la base de datos
/// </summary>
public class CommandExecutorService : ICommandExecutorService
{
    private readonly IExpenseIncomeService _expenseIncomeService;
    private readonly ICategoryService _categoryService;
    private readonly IBudgetService _budgetService;
    private readonly IFinancialGoalService _goalService;
    private readonly ILoanService _loanService;

    public CommandExecutorService(
        IExpenseIncomeService expenseIncomeService,
        ICategoryService categoryService,
        IBudgetService budgetService,
        IFinancialGoalService goalService,
        ILoanService loanService)
    {
        _expenseIncomeService = expenseIncomeService;
        _categoryService = categoryService;
        _budgetService = budgetService;
        _goalService = goalService;
        _loanService = loanService;
    }

    public async Task<CommandResultDto> ExecuteCommandAsync(CommandDto command, string userId)
    {
        try
        {
            return command.Type switch
            {
                // Gastos e Ingresos
                CommandType.CreateExpense => await CreateExpenseAsync(command, userId),
                CommandType.CreateIncome => await CreateIncomeAsync(command, userId),
                CommandType.DeleteLastExpense => await DeleteLastExpenseAsync(userId),
                CommandType.DeleteLastIncome => await DeleteLastIncomeAsync(userId),
                CommandType.GetBalance => await GetBalanceAsync(userId),
                CommandType.GetExpensesByCategory => await GetExpensesByCategoryAsync(command, userId),
                CommandType.GetIncomesByCategory => await GetIncomesByCategoryAsync(command, userId),
                CommandType.GetRecentTransactions => await GetRecentTransactionsAsync(command, userId),

                // Presupuestos
                CommandType.CreateBudget => await CreateBudgetAsync(command, userId),
                CommandType.UpdateBudget => await UpdateBudgetAsync(command, userId),
                CommandType.DeleteBudget => await DeleteBudgetAsync(command, userId),
                CommandType.GetBudgetStatus => await GetBudgetStatusAsync(command, userId),
                CommandType.GetAllBudgets => await GetAllBudgetsAsync(userId),
                CommandType.GetBudgetsByStatus => await GetBudgetsByStatusAsync(command, userId),

                // Metas
                CommandType.CreateGoal => await CreateGoalAsync(command, userId),
                CommandType.AddToGoal => await AddToGoalAsync(command, userId),
                CommandType.WithdrawFromGoal => await WithdrawFromGoalAsync(command, userId),
                CommandType.CompleteGoal => await CompleteGoalAsync(command, userId),
                CommandType.CancelGoal => await CancelGoalAsync(command, userId),
                CommandType.GetGoalProgress => await GetGoalProgressAsync(command, userId),
                CommandType.GetAllGoals => await GetAllGoalsAsync(userId),

                // Préstamos
                CommandType.CreateLoan => await CreateLoanAsync(command, userId),
                CommandType.RegisterLoanPayment => await RegisterLoanPaymentAsync(command, userId),
                CommandType.UndoLoanPayment => await UndoLoanPaymentAsync(command, userId),
                CommandType.GetLoanStatus => await GetLoanStatusAsync(command, userId),
                CommandType.GetAllLoans => await GetAllLoansAsync(userId),
                CommandType.GetUpcomingPayments => await GetUpcomingPaymentsAsync(command, userId),
                CommandType.GetTotalDebt => await GetTotalDebtAsync(userId),

                // Categorías
                CommandType.CreateCategory => await CreateCategoryAsync(command, userId),
                CommandType.GetCategories => await GetCategoriesAsync(command, userId),

                // Análisis
                CommandType.GetMonthSummary => await GetMonthSummaryAsync(command, userId),
                CommandType.CompareMonths => await CompareMonthsAsync(command, userId),
                CommandType.GetTopExpenseCategories => await GetTopExpenseCategoriesAsync(command, userId),
                CommandType.GetSpendingTrend => await GetSpendingTrendAsync(command, userId),
                CommandType.GetSavingsRate => await GetSavingsRateAsync(userId),

                _ => new CommandResultDto
                {
                    Success = false,
                    Message = "Comando no soportado",
                    ErrorDetails = $"El tipo de comando {command.Type} no está implementado"
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ejecutando comando {command.Type}: {ex.Message}");
            return new CommandResultDto
            {
                Success = false,
                Message = "Ocurrió un error al ejecutar el comando",
                ErrorDetails = ex.Message
            };
        }
    }

    #region Gastos e Ingresos

    private async Task<CommandResultDto> CreateExpenseAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("amount", out var amountObj) ||
                !command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos (monto o categoría)" };
            }

            var amount = Convert.ToDecimal(amountObj);
            var categoryName = categoryObj.ToString() ?? "";
            var description = command.Parameters.TryGetValue("description", out var descObj) ? descObj.ToString() : "";
            var dateStr = command.Parameters.TryGetValue("date", out var dateObj) ? dateObj.ToString() : "hoy";

            // --- LÓGICA NUEVA: OBTENER O CREAR ---
            var categoryResult = await GetOrCreateCategoryAsync(
                categoryName,
                userId,
                command.CreateCategoryIfMissing,
                command.SuggestedIcon,
                TransactionType.Expense
            );

            if (categoryResult.Id == null)
            {
                return new CommandResultDto
                {
                    Success = false,
                    Message = $"No se encontró la categoría '{categoryName}' y no se pudo crear automáticamente.",
                    ErrorDetails = "Intenta crear la categoría manualmente primero."
                };
            }
            // -------------------------------------

            var date = ParseDate(dateStr);

            var expenseDto = new ExpenseIncomeDto
            {
                Amount = amount,
                CategoryId = categoryResult.Id.Value, // Usar el ID resuelto
                Type = TransactionType.Expense,
                Date = date,
                Description = description ?? $"Gasto en {categoryResult.Name}"
            };

            var result = await _expenseIncomeService.CreateAsync(expenseDto, userId);

            if (result.Success)
            {
                var balance = await _expenseIncomeService.GetBalanceByUserAsync(userId);

                // Mensaje personalizado si se creó la categoría
                var msg = categoryResult.WasCreated
                    ? $"✅ He creado la categoría **{command.SuggestedIcon} {categoryResult.Name}** y registrado el gasto de RD${amount:N2}."
                    : $"✅ Gasto creado: RD${amount:N2} en {categoryResult.Name}\n\n💰 Balance actual: RD${balance:N2}";

                return new CommandResultDto
                {
                    Success = true,
                    Message = msg,
                    Data = new { ExpenseId = result.Data?.Id, NewBalance = balance }
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear el gasto", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear el gasto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> CreateIncomeAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("amount", out var amountObj) ||
                !command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var amount = Convert.ToDecimal(amountObj);
            var categoryName = categoryObj.ToString() ?? "";
            var description = command.Parameters.TryGetValue("description", out var descObj) ? descObj.ToString() : "";
            var dateStr = command.Parameters.TryGetValue("date", out var dateObj) ? dateObj.ToString() : "hoy";

            // --- LÓGICA NUEVA: OBTENER O CREAR ---
            var categoryResult = await GetOrCreateCategoryAsync(
                categoryName,
                userId,
                command.CreateCategoryIfMissing,
                command.SuggestedIcon,
                TransactionType.Income
            );

            if (categoryResult.Id == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la categoría '{categoryName}'" };
            }
            // -------------------------------------

            var date = ParseDate(dateStr);
            var incomeDto = new ExpenseIncomeDto
            {
                Amount = amount,
                CategoryId = categoryResult.Id.Value,
                Type = TransactionType.Income,
                Date = date,
                Description = description ?? $"Ingreso por {categoryResult.Name}"
            };

            var result = await _expenseIncomeService.CreateAsync(incomeDto, userId);

            if (result.Success)
            {
                var balance = await _expenseIncomeService.GetBalanceByUserAsync(userId);

                var msg = categoryResult.WasCreated
                    ? $"✅ He creado la categoría **{command.SuggestedIcon} {categoryResult.Name}** y registrado el ingreso de RD${amount:N2}."
                    : $"✅ Ingreso creado: RD${amount:N2} en {categoryResult.Name}\n\n💰 Balance actual: RD${balance:N2}";

                return new CommandResultDto
                {
                    Success = true,
                    Message = msg,
                    Data = new { IncomeId = result.Data?.Id, NewBalance = balance }
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear el ingreso", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear el ingreso", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> DeleteLastExpenseAsync(string userId)
    {
        try
        {
            var transactions = await _expenseIncomeService.GetByUserAndTypeAsync(userId, TransactionType.Expense);
            var lastExpense = transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).FirstOrDefault();

            if (lastExpense == null)
            {
                return new CommandResultDto { Success = false, Message = "No tienes gastos registrados para eliminar" };
            }

            var deleted = await _expenseIncomeService.DeleteAsync(lastExpense.Id, userId);

            if (deleted)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Gasto eliminado: RD${lastExpense.Amount:N2} en {lastExpense.CategoryTitle} ({lastExpense.Date:dd/MM/yyyy})"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo eliminar el gasto" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al eliminar el gasto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> DeleteLastIncomeAsync(string userId)
    {
        try
        {
            var transactions = await _expenseIncomeService.GetByUserAndTypeAsync(userId, TransactionType.Income);
            var lastIncome = transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id).FirstOrDefault();

            if (lastIncome == null)
            {
                return new CommandResultDto { Success = false, Message = "No tienes ingresos registrados para eliminar" };
            }

            var deleted = await _expenseIncomeService.DeleteAsync(lastIncome.Id, userId);

            if (deleted)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Ingreso eliminado: RD${lastIncome.Amount:N2} en {lastIncome.CategoryTitle} ({lastIncome.Date:dd/MM/yyyy})"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo eliminar el ingreso" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al eliminar el ingreso", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetBalanceAsync(string userId)
    {
        try
        {
            var balance = await _expenseIncomeService.GetBalanceByUserAsync(userId);
            var totalIncome = await _expenseIncomeService.GetTotalIngresosByUserAsync(userId);
            var totalExpense = await _expenseIncomeService.GetTotalGastosByUserAsync(userId);

            var message = balance >= 0
                ? $"💰 Tu balance actual es: **RD${balance:N2}**\n\n📈 Total ingresos: RD${totalIncome:N2}\n📉 Total gastos: RD${totalExpense:N2}\n\n✅ Situación financiera positiva"
                : $"💰 Tu balance actual es: **-RD${Math.Abs(balance):N2}**\n\n📈 Total ingresos: RD${totalIncome:N2}\n📉 Total gastos: RD${totalExpense:N2}\n\n⚠️ Tus gastos exceden tus ingresos";

            return new CommandResultDto
            {
                Success = true,
                Message = message,
                Data = new { Balance = balance, TotalIncome = totalIncome, TotalExpense = totalExpense }
            };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar el balance", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetExpensesByCategoryAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar la categoría" };
            }

            var categoryName = categoryObj.ToString() ?? "";
            var period = command.Parameters.TryGetValue("period", out var periodObj) ? periodObj.ToString() : "este mes";
            var (startDate, endDate) = ParsePeriod(period ?? "este mes");

            var categories = await _categoryService.GetByUserAndTypeAsync(userId, TransactionType.Expense);
            var category = categories.FirstOrDefault(c => c.Title.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la categoría '{categoryName}'" };
            }

            var expenses = await _expenseIncomeService.GetByUserAndDateRangeAsync(userId, startDate, endDate);
            var categoryExpenses = expenses.Where(e => e.Type == TransactionType.Expense && e.CategoryId == category.CategoryId).ToList();

            var total = categoryExpenses.Sum(e => e.Amount);
            var count = categoryExpenses.Count;

            var message = count > 0
                ? $"📊 Gastos en {category.Icon} **{categoryName}** ({period}):\n\n💵 Total: **RD${total:N2}**\n🔢 Transacciones: {count}"
                : $"No tienes gastos registrados en {category.Icon} {categoryName} {period}";

            return new CommandResultDto { Success = true, Message = message, Data = new { Category = categoryName, Total = total, Count = count } };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar gastos", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetIncomesByCategoryAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar la categoría" };
            }

            var categoryName = categoryObj.ToString() ?? "";
            var period = command.Parameters.TryGetValue("period", out var periodObj) ? periodObj.ToString() : "este mes";
            var (startDate, endDate) = ParsePeriod(period ?? "este mes");

            var categories = await _categoryService.GetByUserAndTypeAsync(userId, TransactionType.Income);
            var category = categories.FirstOrDefault(c => c.Title.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la categoría '{categoryName}'" };
            }

            var incomes = await _expenseIncomeService.GetByUserAndDateRangeAsync(userId, startDate, endDate);
            var categoryIncomes = incomes.Where(i => i.Type == TransactionType.Income && i.CategoryId == category.CategoryId).ToList();

            var total = categoryIncomes.Sum(i => i.Amount);
            var count = categoryIncomes.Count;

            var message = count > 0
                ? $"📊 Ingresos en {category.Icon} **{categoryName}** ({period}):\n\n💵 Total: **RD${total:N2}**\n🔢 Transacciones: {count}"
                : $"No tienes ingresos registrados en {category.Icon} {categoryName} {period}";

            return new CommandResultDto { Success = true, Message = message, Data = new { Category = categoryName, Total = total, Count = count } };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar ingresos", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetRecentTransactionsAsync(CommandDto command, string userId)
    {
        try
        {
            var count = command.Parameters.TryGetValue("count", out var countObj) ? Convert.ToInt32(countObj) : 10;
            var transactions = await _expenseIncomeService.GetRecentTransactionsAsync(userId, count);

            if (!transactions.Any())
            {
                return new CommandResultDto { Success = true, Message = "No tienes transacciones registradas" };
            }

            var message = $"📋 **Últimas {transactions.Count} transacciones:**\n\n";
            foreach (var t in transactions)
            {
                var icon = t.Type == TransactionType.Income ? "📈" : "📉";
                message += $"{icon} RD${t.Amount:N2} - {t.CategoryTitle} ({t.Date:dd/MM})\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = transactions };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar transacciones", ErrorDetails = ex.Message };
        }
    }

    #endregion

    #region Presupuestos

    private async Task<CommandResultDto> CreateBudgetAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("amount", out var amountObj) ||
                !command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var amount = Convert.ToDecimal(amountObj);
            var categoryName = categoryObj.ToString() ?? "";
            var month = command.Parameters.TryGetValue("month", out var monthObj) ? Convert.ToInt32(monthObj) : DateTime.Now.Month;
            var year = command.Parameters.TryGetValue("year", out var yearObj) ? Convert.ToInt32(yearObj) : DateTime.Now.Year;

            var categories = await _categoryService.GetByUserAndTypeAsync(userId, TransactionType.Expense);
            var category = categories.FirstOrDefault(c => c.Title.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la categoría '{categoryName}'" };
            }

            var budgetDto = new BudgetDto
            {
                CategoryId = category.CategoryId,
                AssignedAmount = amount,
                Month = month,
                Year = year
            };

            var result = await _budgetService.CreateAsync(budgetDto, userId);

            if (result.Success)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Presupuesto creado: RD${amount:N2} para {category.Icon} {categoryName} ({month:00}/{year})"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear el presupuesto", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear presupuesto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> UpdateBudgetAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("category", out var categoryObj) ||
                !command.Parameters.TryGetValue("newAmount", out var amountObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var categoryName = categoryObj.ToString() ?? "";
            var newAmount = Convert.ToDecimal(amountObj);
            var now = DateTime.Now;

            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, now.Month, now.Year);
            var budget = budgets.FirstOrDefault(b => b.CategoryTitle.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (budget == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró presupuesto para '{categoryName}' en el mes actual" };
            }

            budget.AssignedAmount = newAmount;
            var updated = await _budgetService.UpdateAsync(budget.Id, budget, userId);

            if (updated)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Presupuesto actualizado: {categoryName} ahora es RD${newAmount:N2}"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo actualizar el presupuesto" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al actualizar presupuesto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> DeleteBudgetAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar la categoría" };
            }

            var categoryName = categoryObj.ToString() ?? "";
            var now = DateTime.Now;

            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, now.Month, now.Year);
            var budget = budgets.FirstOrDefault(b => b.CategoryTitle.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (budget == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró presupuesto para '{categoryName}'" };
            }

            var deleted = await _budgetService.DeleteAsync(budget.Id, userId);

            if (deleted)
            {
                return new CommandResultDto { Success = true, Message = $"✅ Presupuesto de {categoryName} eliminado" };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo eliminar el presupuesto" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al eliminar presupuesto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetBudgetStatusAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("category", out var categoryObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar la categoría" };
            }

            var categoryName = categoryObj.ToString() ?? "";
            var now = DateTime.Now;

            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, now.Month, now.Year);
            var budget = budgets.FirstOrDefault(b => b.CategoryTitle.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (budget == null)
            {
                return new CommandResultDto { Success = false, Message = $"No tienes presupuesto para '{categoryName}' este mes" };
            }

            var percentage = budget.AssignedAmount > 0 ? (budget.SpentAmount / budget.AssignedAmount * 100) : 0;
            var remaining = budget.AssignedAmount - budget.SpentAmount;
            var status = percentage >= 100 ? "⚠️ EXCEDIDO" : percentage >= 80 ? "⚡ Cerca del límite" : "✅ Normal";

            var message = $"📊 **Presupuesto de {budget.CategoryTitle}**\n\n" +
                         $"💵 Asignado: RD${budget.AssignedAmount:N2}\n" +
                         $"📉 Gastado: RD${budget.SpentAmount:N2}\n" +
                         $"💰 Disponible: RD${remaining:N2}\n" +
                         $"📈 Usado: {percentage:F1}%\n" +
                         $"🔔 Estado: {status}";

            return new CommandResultDto { Success = true, Message = message, Data = budget };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar presupuesto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetAllBudgetsAsync(string userId)
    {
        try
        {
            var now = DateTime.Now;
            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, now.Month, now.Year);

            if (!budgets.Any())
            {
                return new CommandResultDto { Success = true, Message = "No tienes presupuestos configurados para este mes" };
            }

            var message = $"📊 **Presupuestos de {now:MMMM yyyy}**\n\n";
            foreach (var b in budgets)
            {
                var percentage = b.AssignedAmount > 0 ? (b.SpentAmount / b.AssignedAmount * 100) : 0;
                var icon = percentage >= 100 ? "🔴" : percentage >= 80 ? "🟡" : "🟢";
                message += $"{icon} {b.CategoryTitle}: RD${b.SpentAmount:N2} / RD${b.AssignedAmount:N2} ({percentage:F0}%)\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = budgets };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar presupuestos", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetBudgetsByStatusAsync(CommandDto command, string userId)
    {
        try
        {
            var status = command.Parameters.TryGetValue("status", out var statusObj) ? statusObj.ToString() : "excedidos";
            var now = DateTime.Now;
            var budgets = await _budgetService.GetByUserAndPeriodAsync(userId, now.Month, now.Year);

            var filtered = status?.ToLower() switch
            {
                "excedidos" => budgets.Where(b => b.SpentAmount >= b.AssignedAmount).ToList(),
                "cerca del límite" or "cerca" => budgets.Where(b => b.SpentAmount < b.AssignedAmount && (b.SpentAmount / b.AssignedAmount) >= 0.8m).ToList(),
                "normales" => budgets.Where(b => (b.SpentAmount / b.AssignedAmount) < 0.8m).ToList(),
                _ => budgets
            };

            if (!filtered.Any())
            {
                return new CommandResultDto { Success = true, Message = $"No hay presupuestos {status}" };
            }

            var message = $"📊 **Presupuestos {status}:**\n\n";
            foreach (var b in filtered)
            {
                var percentage = b.AssignedAmount > 0 ? (b.SpentAmount / b.AssignedAmount * 100) : 0;
                message += $"• {b.CategoryTitle}: {percentage:F0}% (RD${b.SpentAmount:N2} / RD${b.AssignedAmount:N2})\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = filtered };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar presupuestos", ErrorDetails = ex.Message };
        }
    }

    #endregion

    #region Metas Financieras

    private async Task<CommandResultDto> CreateGoalAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("title", out var titleObj) ||
                !command.Parameters.TryGetValue("targetAmount", out var amountObj) ||
                !command.Parameters.TryGetValue("targetDate", out var dateObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var title = titleObj.ToString() ?? "";
            var targetAmount = Convert.ToDecimal(amountObj);
            var targetDate = ParseDate(dateObj.ToString());
            var currentAmount = command.Parameters.TryGetValue("currentAmount", out var currentObj) ? Convert.ToDecimal(currentObj) : 0;

            var goalDto = new FinancialGoalDto
            {
                Title = title,
                TargetAmount = targetAmount,
                CurrentAmount = currentAmount,
                TargetDate = targetDate,
                Status = GoalStatus.InProgress
            };

            var result = await _goalService.CreateAsync(goalDto, userId);

            if (result.Success)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Meta creada: **{title}**\n\n🎯 Objetivo: RD${targetAmount:N2}\n📅 Fecha límite: {targetDate:dd/MM/yyyy}\n💰 Inicial: RD${currentAmount:N2}"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear la meta", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear meta", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> AddToGoalAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("goalTitle", out var titleObj) ||
                !command.Parameters.TryGetValue("amount", out var amountObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var goalTitle = titleObj.ToString() ?? "";
            var amount = Convert.ToDecimal(amountObj);

            var goals = await _goalService.GetActiveByUserAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Title.Equals(goalTitle, StringComparison.OrdinalIgnoreCase));

            if (goal == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la meta '{goalTitle}'" };
            }

            var result = await _goalService.AddProgressAsync(goal.GoalId, amount, userId);

            if (result.Success)
            {
                var newCurrent = goal.CurrentAmount + amount;
                var progress = goal.TargetAmount > 0 ? (newCurrent / goal.TargetAmount * 100) : 0;
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Agregado RD${amount:N2} a **{goal.Title}**\n\n💰 Nuevo saldo: RD${newCurrent:N2} / RD${goal.TargetAmount:N2}\n📈 Progreso: {progress:F1}%"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo agregar el monto", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al agregar monto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> WithdrawFromGoalAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("goalTitle", out var titleObj) ||
                !command.Parameters.TryGetValue("amount", out var amountObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var goalTitle = titleObj.ToString() ?? "";
            var amount = Convert.ToDecimal(amountObj);

            var goals = await _goalService.GetActiveByUserAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Title.Equals(goalTitle, StringComparison.OrdinalIgnoreCase));

            if (goal == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la meta '{goalTitle}'" };
            }

            var result = await _goalService.WithdrawAmountAsync(goal.GoalId, amount, userId);

            if (result.Success)
            {
                var newCurrent = goal.CurrentAmount - amount;
                var progress = goal.TargetAmount > 0 ? (newCurrent / goal.TargetAmount * 100) : 0;
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Retirado RD${amount:N2} de **{goal.Title}**\n\n💰 Nuevo saldo: RD${newCurrent:N2} / RD${goal.TargetAmount:N2}\n📉 Progreso: {progress:F1}%"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo retirar el monto", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al retirar monto", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> CompleteGoalAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("goalTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título de la meta" };
            }

            var goalTitle = titleObj.ToString() ?? "";
            var goals = await _goalService.GetActiveByUserAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Title.Equals(goalTitle, StringComparison.OrdinalIgnoreCase));

            if (goal == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la meta '{goalTitle}'" };
            }

            var completed = await _goalService.CompleteGoalAsync(goal.GoalId, userId);

            if (completed)
            {
                return new CommandResultDto { Success = true, Message = $"🎉 ¡Felicidades! Meta **{goal.Title}** completada" };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo completar la meta" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al completar meta", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> CancelGoalAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("goalTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título de la meta" };
            }

            var goalTitle = titleObj.ToString() ?? "";
            var goals = await _goalService.GetActiveByUserAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Title.Equals(goalTitle, StringComparison.OrdinalIgnoreCase));

            if (goal == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la meta '{goalTitle}'" };
            }

            var cancelled = await _goalService.CancelGoalAsync(goal.GoalId, userId);

            if (cancelled)
            {
                return new CommandResultDto { Success = true, Message = $"✅ Meta **{goal.Title}** cancelada" };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo cancelar la meta" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al cancelar meta", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetGoalProgressAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("goalTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título de la meta" };
            }

            var goalTitle = titleObj.ToString() ?? "";
            var goals = await _goalService.GetAllByUserAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Title.Equals(goalTitle, StringComparison.OrdinalIgnoreCase));

            if (goal == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró la meta '{goalTitle}'" };
            }

            var progress = goal.TargetAmount > 0 ? (goal.CurrentAmount / goal.TargetAmount * 100) : 0;
            var remaining = goal.TargetAmount - goal.CurrentAmount;
            var daysLeft = (goal.TargetDate - DateTime.Now).Days;

            var message = $"🎯 **{goal.Title}**\n\n" +
                         $"💰 Ahorrado: RD${goal.CurrentAmount:N2} / RD${goal.TargetAmount:N2}\n" +
                         $"📈 Progreso: {progress:F1}%\n" +
                         $"💵 Falta: RD${remaining:N2}\n" +
                         $"📅 Días restantes: {daysLeft}\n" +
                         $"🔔 Estado: {goal.Status}";

            return new CommandResultDto { Success = true, Message = message, Data = goal };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar meta", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetAllGoalsAsync(string userId)
    {
        try
        {
            var goals = await _goalService.GetActiveByUserAsync(userId);

            if (!goals.Any())
            {
                return new CommandResultDto { Success = true, Message = "No tienes metas activas" };
            }

            var message = "🎯 **Metas activas:**\n\n";
            foreach (var g in goals)
            {
                var progress = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount * 100) : 0;
                message += $"• **{g.Title}**: {progress:F0}% (RD${g.CurrentAmount:N2} / RD${g.TargetAmount:N2})\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = goals };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar metas", ErrorDetails = ex.Message };
        }
    }

    #endregion

    #region Préstamos

    private async Task<CommandResultDto> CreateLoanAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("title", out var titleObj) ||
                !command.Parameters.TryGetValue("principalAmount", out var principalObj) ||
                !command.Parameters.TryGetValue("installmentAmount", out var installmentObj) ||
                !command.Parameters.TryGetValue("numberOfInstallments", out var numInstallmentsObj) ||
                !command.Parameters.TryGetValue("dueDay", out var dueDayObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var title = titleObj.ToString() ?? "";
            var principalAmount = Convert.ToDecimal(principalObj);
            var installmentAmount = Convert.ToDecimal(installmentObj);
            var numberOfInstallments = Convert.ToInt32(numInstallmentsObj);
            var dueDay = Convert.ToInt32(dueDayObj);

            var loanDto = new LoanDto
            {
                Title = title,
                PrincipalAmount = principalAmount,
                InstallmentAmount = installmentAmount,
                NumberOfInstallments = numberOfInstallments,
                DueDay = dueDay,
                StartDate = DateTime.Today
            };

            var result = await _loanService.CreateAsync(loanDto, userId);

            if (result.Success)
            {
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Préstamo **{title}** creado\n\n💰 Monto: RD${principalAmount:N2}\n📅 Cuota: RD${installmentAmount:N2}\n🔢 Cantidad: {numberOfInstallments} cuotas\n📆 Día de pago: {dueDay}"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear el préstamo", ErrorDetails = result.Error };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear préstamo", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> RegisterLoanPaymentAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("loanTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título del préstamo" };
            }

            var loanTitle = titleObj.ToString() ?? "";
            var loans = await _loanService.GetActiveByUserAsync(userId);
            var loan = loans.FirstOrDefault(l => l.Title.Equals(loanTitle, StringComparison.OrdinalIgnoreCase));

            if (loan == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró el préstamo '{loanTitle}'" };
            }

            var registered = await _loanService.RegisterPaymentAsync(loan.LoanId, userId);

            if (registered)
            {
                var remaining = loan.NumberOfInstallments - (loan.InstallmentsPaid + 1);
                return new CommandResultDto
                {
                    Success = true,
                    Message = $"✅ Pago registrado para **{loan.Title}**\n\n💰 Cuota: RD${loan.InstallmentAmount:N2}\n🔢 Cuotas restantes: {remaining}"
                };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo registrar el pago" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al registrar pago", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> UndoLoanPaymentAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("loanTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título del préstamo" };
            }

            var loanTitle = titleObj.ToString() ?? "";
            var loans = await _loanService.GetActiveByUserAsync(userId);
            var loan = loans.FirstOrDefault(l => l.Title.Equals(loanTitle, StringComparison.OrdinalIgnoreCase));

            if (loan == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró el préstamo '{loanTitle}'" };
            }

            var undone = await _loanService.UndoLastPaymentAsync(loan.LoanId, userId);

            if (undone)
            {
                return new CommandResultDto { Success = true, Message = $"✅ Último pago de **{loan.Title}** deshecho" };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo deshacer el pago" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al deshacer pago", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetLoanStatusAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("loanTitle", out var titleObj))
            {
                return new CommandResultDto { Success = false, Message = "Falta especificar el título del préstamo" };
            }

            var loanTitle = titleObj.ToString() ?? "";
            var loans = await _loanService.GetAllByUserAsync(userId);
            var loan = loans.FirstOrDefault(l => l.Title.Equals(loanTitle, StringComparison.OrdinalIgnoreCase));

            if (loan == null)
            {
                return new CommandResultDto { Success = false, Message = $"No se encontró el préstamo '{loanTitle}'" };
            }

            var remaining = loan.NumberOfInstallments - loan.InstallmentsPaid;
            var totalPaid = loan.InstallmentAmount * loan.InstallmentsPaid;

            var message = $"💳 **{loan.Title}**\n\n" +
                         $"💰 Monto original: RD${loan.PrincipalAmount:N2}\n" +
                         $"📅 Cuota: RD${loan.InstallmentAmount:N2}\n" +
                         $"✅ Cuotas pagadas: {loan.InstallmentsPaid} / {loan.NumberOfInstallments}\n" +
                         $"⏳ Cuotas restantes: {remaining}\n" +
                         $"💵 Total pagado: RD${totalPaid:N2}\n" +
                         $"💰 Saldo actual: RD${loan.CurrentBalance:N2}";

            return new CommandResultDto { Success = true, Message = message, Data = loan };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar préstamo", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetAllLoansAsync(string userId)
    {
        try
        {
            var loans = await _loanService.GetActiveByUserAsync(userId);

            if (!loans.Any())
            {
                return new CommandResultDto { Success = true, Message = "No tienes préstamos activos" };
            }

            var message = "💳 **Préstamos activos:**\n\n";
            foreach (var l in loans)
            {
                var remaining = l.NumberOfInstallments - l.InstallmentsPaid;
                message += $"• **{l.Title}**: {l.InstallmentsPaid}/{l.NumberOfInstallments} cuotas ({remaining} restantes) - Saldo: RD${l.CurrentBalance:N2}\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = loans };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar préstamos", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetUpcomingPaymentsAsync(CommandDto command, string userId)
    {
        try
        {
            var daysAhead = command.Parameters.TryGetValue("daysAhead", out var daysObj) ? Convert.ToInt32(daysObj) : 7;
            var loans = await _loanService.GetLoansWithUpcomingPaymentsAsync(userId, daysAhead);

            if (!loans.Any())
            {
                return new CommandResultDto { Success = true, Message = $"No tienes pagos de préstamos en los próximos {daysAhead} días" };
            }

            var message = $"📅 **Próximos pagos ({daysAhead} días):**\n\n";
            foreach (var l in loans)
            {
                message += $"• **{l.Title}**: RD${l.InstallmentAmount:N2} (Día {l.DueDay})\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = loans };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar próximos pagos", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetTotalDebtAsync(string userId)
    {
        try
        {
            var totalBorrowed = await _loanService.GetTotalBorrowedAsync(userId);
            var totalToPay = await _loanService.GetTotalToPayAsync(userId);
            var totalRemaining = await _loanService.GetTotalRemainingAsync(userId);
            var monthlyPayments = await _loanService.GetMonthlyPaymentsTotalAsync(userId);

            var message = $"💳 **Resumen de deudas:**\n\n" +
                         $"💰 Total prestado: RD${totalBorrowed:N2}\n" +
                         $"📊 Total a pagar: RD${totalToPay:N2}\n" +
                         $"⏳ Saldo restante: RD${totalRemaining:N2}\n" +
                         $"📅 Pagos mensuales: RD${monthlyPayments:N2}";

            return new CommandResultDto
            {
                Success = true,
                Message = message,
                Data = new { TotalBorrowed = totalBorrowed, TotalToPay = totalToPay, TotalRemaining = totalRemaining, MonthlyPayments = monthlyPayments }
            };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar deudas", ErrorDetails = ex.Message };
        }
    }

    #endregion

    #region Categorías

    private async Task<CommandResultDto> CreateCategoryAsync(CommandDto command, string userId)
    {
        try
        {
            if (!command.Parameters.TryGetValue("title", out var titleObj) ||
                !command.Parameters.TryGetValue("icon", out var iconObj) ||
                !command.Parameters.TryGetValue("type", out var typeObj))
            {
                return new CommandResultDto { Success = false, Message = "Faltan parámetros requeridos" };
            }

            var title = titleObj.ToString() ?? "";
            var icon = iconObj.ToString() ?? "📁";
            var typeStr = typeObj.ToString() ?? "";

            if (!Enum.TryParse<TransactionType>(typeStr, out var type))
            {
                return new CommandResultDto { Success = false, Message = "Tipo de categoría inválido (debe ser Expense o Income)" };
            }

            // Verificar si ya existe una categoría con ese nombre
            var exists = await _categoryService.ExistsCategoryWithNameAsync(title, type, userId);
            if (exists)
            {
                return new CommandResultDto { Success = false, Message = $"Ya existe una categoría con el nombre '{title}'" };
            }

            var categoryDto = new CategoryDto
            {
                Title = title,
                Icon = icon,
                Type = type
            };

            var createdCategory = await _categoryService.CreateAsync(categoryDto, userId);

            if (createdCategory != null && createdCategory.CategoryId > 0)
            {
                return new CommandResultDto { Success = true, Message = $"✅ Categoría **{icon} {title}** creada exitosamente" };
            }

            return new CommandResultDto { Success = false, Message = "No se pudo crear la categoría" };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al crear categoría", ErrorDetails = ex.Message };
        }
    }


    private async Task<CommandResultDto> GetCategoriesAsync(CommandDto command, string userId)
    {
        try
        {
            var typeFilter = command.Parameters.TryGetValue("type", out var typeObj) ? typeObj.ToString() : "All";

            List<CategoryDto> categories;

            if (typeFilter == "Expense")
            {
                categories = await _categoryService.GetByUserAndTypeAsync(userId, TransactionType.Expense);
            }
            else if (typeFilter == "Income")
            {
                categories = await _categoryService.GetByUserAndTypeAsync(userId, TransactionType.Income);
            }
            else
            {
                categories = await _categoryService.GetAllByUserAsync(userId);
            }

            if (!categories.Any())
            {
                return new CommandResultDto { Success = true, Message = "No tienes categorías registradas" };
            }

            var message = $"📁 **Categorías {typeFilter}:**\n\n";
            foreach (var c in categories)
            {
                message += $"• {c.Icon} {c.Title} ({c.Type})\n";
            }

            return new CommandResultDto { Success = true, Message = message, Data = categories };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al consultar categorías", ErrorDetails = ex.Message };
        }
    }

    #endregion

    #region Análisis y Reportes

    private async Task<CommandResultDto> GetMonthSummaryAsync(CommandDto command, string userId)
    {
        try
        {
            var now = DateTime.Now;
            var month = command.Parameters.TryGetValue("month", out var monthObj) ? Convert.ToInt32(monthObj) : now.Month;
            var year = command.Parameters.TryGetValue("year", out var yearObj) ? Convert.ToInt32(yearObj) : now.Year;

            var (ingresos, gastos) = await _expenseIncomeService.GetTotalsByMonthAsync(userId, month, year);
            var balance = ingresos - gastos;

            var message = $"📊 **Resumen de {new DateTime(year, month, 1):MMMM yyyy}**\n\n" +
                         $"📈 Ingresos: RD${ingresos:N2}\n" +
                         $"📉 Gastos: RD${gastos:N2}\n" +
                         $"💰 Balance: RD${balance:N2}\n" +
                         $"🔔 Estado: {(balance >= 0 ? "✅ Positivo" : "⚠️ Negativo")}";

            return new CommandResultDto { Success = true, Message = message, Data = new { Ingresos = ingresos, Gastos = gastos, Balance = balance } };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al generar resumen", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> CompareMonthsAsync(CommandDto command, string userId)
    {
        try
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var (currentIncome, currentExpense) = await _expenseIncomeService.GetTotalsByMonthAsync(userId, currentMonth, currentYear);
            var (previousIncome, previousExpense) = await _expenseIncomeService.GetTotalsByMonthAsync(userId, previousMonth, previousYear);

            var incomeDiff = currentIncome - previousIncome;
            var expenseDiff = currentExpense - previousExpense;

            var message = $"📊 **Comparación de meses**\n\n" +
                         $"**{new DateTime(currentYear, currentMonth, 1):MMMM}:**\n" +
                         $"📈 Ingresos: RD${currentIncome:N2}\n" +
                         $"📉 Gastos: RD${currentExpense:N2}\n\n" +
                         $"**{new DateTime(previousYear, previousMonth, 1):MMMM}:**\n" +
                         $"📈 Ingresos: RD${previousIncome:N2}\n" +
                         $"📉 Gastos: RD${previousExpense:N2}\n\n" +
                         $"**Diferencias:**\n" +
                         $"📊 Ingresos: {(incomeDiff >= 0 ? "+" : "")}{incomeDiff:N2} ({(incomeDiff >= 0 ? "📈" : "📉")})\n" +
                         $"📊 Gastos: {(expenseDiff >= 0 ? "+" : "")}{expenseDiff:N2} ({(expenseDiff >= 0 ? "📉" : "📈")})";

            return new CommandResultDto { Success = true, Message = message };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al comparar meses", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetTopExpenseCategoriesAsync(CommandDto command, string userId)
    {
        try
        {
            var limit = command.Parameters.TryGetValue("limit", out var limitObj) ? Convert.ToInt32(limitObj) : 5;
            var period = command.Parameters.TryGetValue("period", out var periodObj) ? periodObj.ToString() : "este mes";
            var (startDate, endDate) = ParsePeriod(period ?? "este mes");

            var expenses = await _expenseIncomeService.GetByUserAndDateRangeAsync(userId, startDate, endDate);
            var expensesByCategory = expenses
                .Where(e => e.Type == TransactionType.Expense)
                .GroupBy(e => e.CategoryTitle)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(limit);

            if (!expensesByCategory.Any())
            {
                return new CommandResultDto { Success = true, Message = $"No hay gastos en {period}" };
            }

            var message = $"📊 **Top {limit} categorías de gasto ({period}):**\n\n";
            var rank = 1;
            foreach (var c in expensesByCategory)
            {
                message += $"{rank}. **{c.Category}**: RD${c.Total:N2}\n";
                rank++;
            }

            return new CommandResultDto { Success = true, Message = message, Data = expensesByCategory };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al obtener top categorías", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetSpendingTrendAsync(CommandDto command, string userId)
    {
        try
        {
            var months = command.Parameters.TryGetValue("months", out var monthsObj) ? Convert.ToInt32(monthsObj) : 3;
            var now = DateTime.Now;
            var trends = new List<(string Month, decimal Expense)>();

            for (int i = 0; i < months; i++)
            {
                var targetDate = now.AddMonths(-i);
                var (_, expense) = await _expenseIncomeService.GetTotalsByMonthAsync(userId, targetDate.Month, targetDate.Year);
                trends.Add((targetDate.ToString("MMM yyyy"), expense));
            }

            trends.Reverse();

            var message = $"📈 **Tendencia de gastos (últimos {months} meses):**\n\n";
            foreach (var t in trends)
            {
                message += $"• {t.Month}: RD${t.Expense:N2}\n";
            }

            var isIncreasing = trends.Count >= 2 && trends[^1].Expense > trends[^2].Expense;
            message += $"\n🔔 Tendencia: {(isIncreasing ? "📈 Creciente" : "📉 Decreciente")}";

            return new CommandResultDto { Success = true, Message = message, Data = trends };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al calcular tendencia", ErrorDetails = ex.Message };
        }
    }

    private async Task<CommandResultDto> GetSavingsRateAsync(string userId)
    {
        try
        {
            var now = DateTime.Now;
            var (income, expense) = await _expenseIncomeService.GetTotalsByMonthAsync(userId, now.Month, now.Year);

            if (income == 0)
            {
                return new CommandResultDto { Success = true, Message = "No tienes ingresos registrados este mes para calcular la tasa de ahorro" };
            }

            var savings = income - expense;
            var savingsRate = (savings / income) * 100;

            var message = $"💰 **Tasa de ahorro ({now:MMMM yyyy}):**\n\n" +
                         $"📈 Ingresos: RD${income:N2}\n" +
                         $"📉 Gastos: RD${expense:N2}\n" +
                         $"💵 Ahorro: RD${savings:N2}\n" +
                         $"📊 Tasa: **{savingsRate:F1}%**\n\n" +
                         $"🔔 {GetSavingsRateMessage(savingsRate)}";

            return new CommandResultDto { Success = true, Message = message, Data = new { SavingsRate = savingsRate, Savings = savings } };
        }
        catch (Exception ex)
        {
            return new CommandResultDto { Success = false, Message = "Error al calcular tasa de ahorro", ErrorDetails = ex.Message };
        }
    }

    private string GetSavingsRateMessage(decimal rate)
    {
        return rate switch
        {
            >= 30 => "🌟 ¡Excelente! Estás ahorrando muy bien",
            >= 20 => "✅ Muy bien, estás en buen camino",
            >= 10 => "👍 Aceptable, intenta mejorar",
            >= 0 => "⚠️ Bajo, deberías reducir gastos",
            _ => "🔴 Crítico, estás gastando más de lo que ganas"
        };
    }

    #endregion

    #region Helpers

    private DateTime ParseDate(string? dateStr)
    {
        dateStr = dateStr?.ToLower() ?? "hoy";

        return dateStr switch
        {
            "hoy" => DateTime.Today,
            "ayer" => DateTime.Today.AddDays(-1),
            "anteayer" => DateTime.Today.AddDays(-2),
            _ => DateTime.TryParse(dateStr, out var parsed) ? parsed : DateTime.Today
        };
    }

    private (DateTime startDate, DateTime endDate) ParsePeriod(string period)
    {
        period = period.ToLower();
        var now = DateTime.Now;
        var today = DateTime.Today;

        return period switch
        {
            "este mes" or "mes actual" => (new DateTime(now.Year, now.Month, 1), now),
            "esta semana" or "semana actual" => (today.AddDays(-(int)today.DayOfWeek), now),
            "hoy" => (today, now),
            "ayer" => (today.AddDays(-1), today.AddDays(-1).AddHours(23).AddMinutes(59)),
            "últimos 7 días" or "ultima semana" => (today.AddDays(-7), now),
            "últimos 30 días" or "ultimo mes" => (today.AddDays(-30), now),
            _ => (new DateTime(now.Year, now.Month, 1), now)
        };
    }

    #endregion

    // Método auxiliar para buscar categorías de forma flexible
    private CategoryDto? FindCategoryFlexible(List<CategoryDto> categories, string searchName)
    {
        if (string.IsNullOrEmpty(searchName)) return null;

        // 1. Intento exacto (case insensitive)
        var exactMatch = categories.FirstOrDefault(c => c.Title.Equals(searchName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null) return exactMatch;

        // 2. Intento "Contiene" (ej: "Supermercado" encaja en "Supermercado y despensa")
        var containsMatch = categories.FirstOrDefault(c => c.Title.Contains(searchName, StringComparison.OrdinalIgnoreCase));
        if (containsMatch != null) return containsMatch;

        // 3. Intento inverso (ej: "Gasto de Comida" encaja en "Comida")
        var reverseMatch = categories.FirstOrDefault(c => searchName.Contains(c.Title, StringComparison.OrdinalIgnoreCase));
        return reverseMatch;
    }

    // Método auxiliar para obtener o crear una categoría
    private async Task<(int? Id, string Name, bool WasCreated)> GetOrCreateCategoryAsync(
        string categoryName,
        string userId,
        bool createIfMissing,
        string? suggestedIcon,
        TransactionType type)
    {
        // --- LOGS DE DEPURACIÓN PARA RENDER ---
        Console.WriteLine($"[Executor] Buscando categoría: '{categoryName}' (Tipo: {type})");
        Console.WriteLine($"[Executor] Autorización para crear si falta: {createIfMissing}");

        // 1. Intentar buscar categorías existentes
        var categories = await _categoryService.GetByUserAndTypeAsync(userId, type);
        var existingCategory = FindCategoryFlexible(categories, categoryName);

        if (existingCategory != null)
        {
            return (existingCategory.CategoryId, existingCategory.Title, false);
        }

        // 2. Si no existe y la orden es crearla
        if (createIfMissing)
        {
            // Capitalizar título (ej: "comida" -> "Comida")
            var titleCase = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(categoryName.ToLower());

            var newCategory = new CategoryDto
            {
                Title = titleCase,
                Icon = suggestedIcon ?? "📁", // Usar el icono sugerido por la IA
                Type = type,
                UserId = userId
            };

            try
            {
                Console.WriteLine($"[CommandExecutor] Auto-creando categoría: {newCategory.Icon} {newCategory.Title}");
                var created = await _categoryService.CreateAsync(newCategory, userId);

                if (created != null && created.CategoryId > 0)
                {
                    return (created.CategoryId, created.Title, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommandExecutor] Error auto-creando categoría: {ex.Message}");
            }
        }

        return (null, categoryName, false);
    }
}
