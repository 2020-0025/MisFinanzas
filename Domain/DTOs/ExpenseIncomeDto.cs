using System.ComponentModel.DataAnnotations;
using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.DTOs
{
    public class ExpenseIncomeDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría válida")]
        public int CategoryId { get; set; }

        public string? CategoryTitle { get; set; }
        public string? CategoryIcon { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Date { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El tipo es requerido")]
        public TransactionType Type { get; set; }

        // Propiedades computadas
        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    TransactionType.Income => "Ingreso",
                    TransactionType.Expense => "Gasto",
                    TransactionType.Adjustment => "Prestamo", // Nuevo tipo
                    _ => "Otro"
                };
            }
        }
        public string FormattedAmount => Amount.ToString("C2");  // RD$1,234.56
    }
}