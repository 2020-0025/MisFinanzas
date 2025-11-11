namespace MisFinanzas.Domain.DTOs
{
    /// Ingreso total por categoría
    public class ReportCategoryIncomeDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
        public int TransactionCount { get; set; }
    }
}