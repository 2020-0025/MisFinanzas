using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class TransactionDto
    {

        public int TransactionId { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría válida")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        [StringLength(500, ErrorMessage = "La nota no puede exceder 500 caracteres")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Date { get; set; } = DateTime.Now;

        // Para mostrar en la UI
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }

        //PROPIEDAD CALCULADA (Computed Property)
        public string FormattedAmount => Amount.ToString("C2");  // RD$1,234.56
    }
}
