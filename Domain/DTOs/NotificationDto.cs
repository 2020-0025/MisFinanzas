namespace MisFinanzas.Domain.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int CategoryId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime NotificationDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Category properties (for display purposes)
        public string? CategoryIcon { get; set; }
        public string? CategoryTitle { get; set; }
        public decimal? CategoryEstimatedAmount { get; set; }

        // Computed Properties
        public int DaysUntilDue => (DueDate.Date - DateTime.Now.Date).Days;

        public bool IsOverdue => DateTime.Now.Date > DueDate.Date;

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
