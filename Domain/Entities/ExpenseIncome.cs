using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.Entities
{
    public class ExpenseIncome
    {
        [Key]
        public int Id { get; set; }

        // Relación con usuario
        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Relación con categoría
        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Información del gasto/ingreso
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Computed Properties
        [NotMapped]
        public string FormattedAmount
        {
            get
            {
                return Type switch
                {
                    TransactionType.Income => $"+{Amount:C2}",
                    TransactionType.Expense => $"-{Amount:C2}",
                    _ => $"{Amount:C2}" // Ajustes sin signo
                };
            }
        }

        [NotMapped]
        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    TransactionType.Income => "Ingreso",
                    TransactionType.Expense => "Gasto",
                    TransactionType.Adjustment => "Ajuste",
                    _ => "Otro"
                };
            }
        }
    }
}