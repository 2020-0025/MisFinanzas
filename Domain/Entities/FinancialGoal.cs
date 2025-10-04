using MisFinanzas.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class FinancialGoal
    {

        [Key]
        public int GoalId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal CurrentAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime TargetDate { get; set; }

        [Required]
        public GoalStatus Status { get; set; } = GoalStatus.InProgress;

        [Required]
        [StringLength(10)]
        public string Icon { get; set; } = "🎯";

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Property
        public virtual ApplicationUser? User { get; set; }

        // Computed Properties
        [NotMapped]
        public decimal ProgressPercentage
        {
            get
            {
                if (TargetAmount == 0) return 0;
                var percentage = (CurrentAmount / TargetAmount) * 100;
                return Math.Min(percentage, 100); // Max 100%
            }
        }

        [NotMapped]
        public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);

        [NotMapped]
        public int DaysRemaining
        {
            get
            {
                var days = (TargetDate - DateTime.Now).Days;
                return Math.Max(days, 0);
            }
        }

        [NotMapped]
        public bool IsCompleted => CurrentAmount >= TargetAmount;

        [NotMapped]
        public bool IsOverdue => DateTime.Now > TargetDate && !IsCompleted;
    }
}
