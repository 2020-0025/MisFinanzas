using MisFinanzas.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(50, ErrorMessage = "El título no puede exceder 50 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El icono es requerido")]
        [StringLength(10)]
        public string Icon { get; set; } = "📁";

        [Required(ErrorMessage = "El tipo es requerido")]
        public TransactionType Type { get; set; } = TransactionType.Expense;

        // Campos para Recordatorios/Gastos Fijos
        public bool IsFixedExpense { get; set; } = false;

        public int? DayOfMonth { get; set; }

        public decimal? EstimatedAmount { get; set; }

        public string TitleWithIcon => $"{Icon} {Title}";

        public string TypeDisplay => Type == TransactionType.Income ? "Ingreso" : "Gasto";
    }
}