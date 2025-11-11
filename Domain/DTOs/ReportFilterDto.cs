using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.DTOs
{
    /// Filtros para generar reportes
    public class ReportFilterDto
    {
        // Período
        public ReportPeriodType PeriodType { get; set; } = ReportPeriodType.LastMonth;
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }

        // Tipo de transacciones
        public TransactionType? TransactionTypeFilter { get; set; } // null = ambos, Income = solo ingresos, Expense = solo gastos

        // Categoría
        public int? CategoryIdFilter { get; set; } // null = todas las categorías

        // Usuario
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}