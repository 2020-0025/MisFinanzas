using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class Transaction
    {

        [Key]
        public int TransactionId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Computed Property
        [NotMapped]
        public string FormattedAmount
        {
            get
            {
                if (Category == null) return $"${Amount:N2}";
                return Category.Type == Domain.Enums.TransactionType.Income
                    ? $"+${Amount:N2}"
                    : $"-${Amount:N2}";
            }
        }
    }
}
