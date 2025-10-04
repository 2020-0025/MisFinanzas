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

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Computed Property
        [NotMapped]
        public string TitleWithIcon => $"{Icon} {Title}";

    }
}
