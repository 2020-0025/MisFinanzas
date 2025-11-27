using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class LoanExtraPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaidDate { get; set; }

        [StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string? Description { get; set; }

        // Navigation Property
        public virtual Loan? Loan { get; set; }
    }
}
