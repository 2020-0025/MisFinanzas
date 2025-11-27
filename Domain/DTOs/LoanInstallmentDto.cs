namespace MisFinanzas.Domain.DTOs
{
    public class LoanInstallmentDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public int? ExpenseIncomeId { get; set; }
        public bool IsRecalculated { get; set; }
        public DateTime? RecalculatedDate { get; set; }

    }
}
