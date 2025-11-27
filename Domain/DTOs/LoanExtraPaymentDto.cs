namespace MisFinanzas.Domain.DTOs
{
    public class LoanExtraPaymentDto
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidDate { get; set; }
        public string? Description { get; set; }
    }
}
