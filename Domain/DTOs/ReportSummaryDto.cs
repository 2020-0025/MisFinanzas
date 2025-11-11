namespace MisFinanzas.Domain.DTOs
{
    /// Resumen estadístico del reporte
    public class ReportSummaryDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public decimal AverageDailyExpense { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalIncomeTransactions { get; set; }
        public int TotalExpenseTransactions { get; set; }
    }
}