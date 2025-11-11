using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MisFinanzas.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime NotificationDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; } // Fecha en que vence el pago

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Category? Category { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Computed Properties
        [NotMapped]
        public int DaysUntilDue => (DueDate.Date - DateTime.Now.Date).Days;

        [NotMapped]
        public bool IsOverdue => DateTime.Now.Date > DueDate.Date;

        [NotMapped]
        public string StatusText
        {
            get
            {
                if (IsOverdue) return "VENCIDO";
                if (DaysUntilDue == 0) return "Vence HOY";
                if (DaysUntilDue == 1) return "Vence MAÑANA";
                return $"Vence en {DaysUntilDue} días";
            }
        }
    }
}