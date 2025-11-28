using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class LoanInstallment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int InstallmentNumber { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }  // Capital de esta cuota

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }   // Interés de esta cuota

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }      // Total de esta cuota

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingBalance { get; set; } // Saldo después de pagar esta cuota

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }

        public int? ExpenseIncomeId { get; set; }     // FK al registro de gasto (solo interés)

        // Navigation Properties
        public virtual Loan? Loan { get; set; }
        public virtual ExpenseIncome? ExpenseIncome { get; set; }
    }
}
