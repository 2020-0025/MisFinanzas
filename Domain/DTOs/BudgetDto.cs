using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class BudgetDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal AssignedAmount { get; set; }

        public decimal SpentAmount { get; set; }

        [Required(ErrorMessage = "El mes es requerido")]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "El año es requerido")]
        [Range(2020, 2100, ErrorMessage = "El año debe estar entre 2020 y 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoryId { get; set; }

        public string CategoryTitle { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = "📁";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedades calculadas
        public decimal AvailableAmount { get; set; }
        public decimal UsedPercentage { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsNearLimit { get; set; }

        // Display
        public string MonthYearDisplay => $"{GetMonthName(Month)} {Year}";

        public string StatusDisplay => IsOverBudget ? "🔴 Excedido" :
                                       IsNearLimit ? "🟡 Cerca del límite" :
                                       "🟢 En control";

        private static string GetMonthName(int month) => month switch
        {
            1 => "Enero",
            2 => "Febrero",
            3 => "Marzo",
            4 => "Abril",
            5 => "Mayo",
            6 => "Junio",
            7 => "Julio",
            8 => "Agosto",
            9 => "Septiembre",
            10 => "Octubre",
            11 => "Noviembre",
            12 => "Diciembre",
            _ => "Desconocido"
        };
    }
}