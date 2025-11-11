using MisFinanzas.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class Category
    {

        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Icon { get; set; } = "📁";

        [Required]
        public TransactionType Type { get; set; } = TransactionType.Expense;

        // Campos para Recordatorios de Gastos Fijos
        public bool IsFixedExpense { get; set; } = false;

        public int? DayOfMonth { get; set; } // 1-31, null si no es gasto fijo

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedAmount { get; set; } // Monto estimado del gasto

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<ExpenseIncome> ExpensesIncomes { get; set; } = new List<ExpenseIncome>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        // Computed Property
        [NotMapped]
        public string TitleWithIcon => $"{Icon} {Title}";

    }
}
