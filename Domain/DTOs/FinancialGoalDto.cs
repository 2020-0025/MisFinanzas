using MisFinanzas.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MisFinanzas.Domain.DTOs
{
    public class FinancialGoalDto
    {

        public int GoalId { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, ErrorMessage = "El título no puede exceder 100 caracteres")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "El monto objetivo es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; } = 0;

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La fecha objetivo es requerida")]
        public DateTime TargetDate { get; set; }

        public GoalStatus Status { get; set; } = GoalStatus.InProgress;

        [Required(ErrorMessage = "El icono es requerido")]
        [StringLength(10)]
        public string Icon { get; set; } = "🎯";

        // Computed properties
        public decimal ProgressPercentage { get; set; }
        public decimal RemainingAmount { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOverdue { get; set; }
    }
}
