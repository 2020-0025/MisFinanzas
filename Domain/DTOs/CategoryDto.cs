using MisFinanzas.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class CategoryDto
    {

        public int CategoryId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(50, ErrorMessage = "El título no puede exceder 50 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "El icono es requerido")]
        [StringLength(10)]
        public string Icon { get; set; } = "📁";

        [Required(ErrorMessage = "El tipo es requerido")]
        public TransactionType Type { get; set; } = TransactionType.Expense;

        public string TitleWithIcon => $"{Icon} {Title}";
    }
}
