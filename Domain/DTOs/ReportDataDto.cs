namespace MisFinanzas.Domain.DTOs
{
    /// Datos completos del reporte
    public class ReportDataDto
    {
        // Información del reporte
        public string UserName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string PeriodDescription { get; set; } = string.Empty;

        // Resumen general
        public ReportSummaryDto Summary { get; set; } = new();

        // Comparativa con período anterior (opcional)
        public ReportComparisonDto? Comparison { get; set; }

        // Gastos por categoría
        public List<ReportCategoryExpenseDto> ExpensesByCategory { get; set; } = new();

        // Ingresos por categoría
        public List<ReportCategoryIncomeDto> IncomesByCategory { get; set; } = new();

        // Transacciones detalladas
        public List<ExpenseIncomeDto> Transactions { get; set; } = new();
    }
}