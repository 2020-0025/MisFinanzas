using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal AssignedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal SpentAmount { get; set; } = 0;

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2020, 2100)]
        public int Year { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual Category? Category { get; set; }

        // Computed Properties
        [NotMapped]
        public decimal AvailableAmount => Math.Max(AssignedAmount - SpentAmount, 0);

        [NotMapped]
        public decimal UsedPercentage => AssignedAmount > 0 ? (SpentAmount / AssignedAmount) * 100 : 0;

        [NotMapped]
        public bool IsOverBudget => SpentAmount > AssignedAmount;

        [NotMapped]
        public bool IsNearLimit => UsedPercentage >= 80 && UsedPercentage < 100;
    }
}