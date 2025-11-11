namespace MisFinanzas.Domain.DTOs
{
    /// Comparativa con período anterior
    public class ReportComparisonDto
    {
        public decimal IncomeChange { get; set; }          // Cambio en ingresos (%)
        public decimal ExpenseChange { get; set; }         // Cambio en gastos (%)
        public decimal BalanceChange { get; set; }         // Cambio en balance (%)
        public string IncomeChangeDisplay { get; set; } = string.Empty;   // "↑ 12%"
        public string ExpenseChangeDisplay { get; set; } = string.Empty;  // "↓ 8%"
        public string BalanceChangeDisplay { get; set; } = string.Empty;  // "↑ 35%"
    }
}