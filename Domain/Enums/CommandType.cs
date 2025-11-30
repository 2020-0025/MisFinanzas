namespace MisFinanzas.Domain.Enums
{
    // Tipos de comandos que el asistente puede ejecutar
    public enum CommandType
    {
        None,

        // ===== GASTOS E INGRESOS =====
        CreateExpense,              // Crear un gasto
        CreateIncome,               // Crear un ingreso
        DeleteLastExpense,          // Eliminar último gasto
        DeleteLastIncome,           // Eliminar último ingreso
        GetBalance,                 // Consultar balance actual
        GetExpensesByCategory,      // Gastos por categoría
        GetIncomesByCategory,       // Ingresos por categoría
        GetRecentTransactions,      // Últimas transacciones

        // ===== PRESUPUESTOS =====
        CreateBudget,               // Crear presupuesto
        UpdateBudget,               // Actualizar presupuesto
        DeleteBudget,               // Eliminar presupuesto
        GetBudgetStatus,            // Estado de un presupuesto
        GetAllBudgets,              // Todos los presupuestos del mes
        GetBudgetsByStatus,         // Presupuestos por estado (excedidos, cerca del límite)

        // ===== METAS FINANCIERAS =====
        CreateGoal,                 // Crear meta de ahorro
        AddToGoal,                  // Agregar dinero a una meta
        WithdrawFromGoal,           // Retirar dinero de una meta
        CompleteGoal,               // Marcar meta como completada
        CancelGoal,                 // Cancelar meta
        GetGoalProgress,            // Progreso de una meta específica
        GetAllGoals,                // Todas las metas activas

        // ===== PRÉSTAMOS =====
        CreateLoan,                 // Crear préstamo
        RegisterLoanPayment,        // Registrar pago de préstamo
        UndoLoanPayment,            // Deshacer último pago
        GetLoanStatus,              // Estado de un préstamo
        GetAllLoans,                // Todos los préstamos activos
        GetUpcomingPayments,        // Próximos pagos de préstamos
        GetTotalDebt,               // Total de deuda

        // ===== CATEGORÍAS =====
        CreateCategory,             // Crear categoría
        DeleteCategory,             // Eliminar categoría
        GetCategories,              // Listar categorías

        // ===== ANÁLISIS Y REPORTES =====
        GetMonthSummary,            // Resumen del mes
        CompareMonths,              // Comparar dos meses
        GetTopExpenseCategories,    // Top categorías de gasto
        GetSpendingTrend,           // Tendencia de gastos
        GetSavingsRate              // Tasa de ahorro
    }
}
