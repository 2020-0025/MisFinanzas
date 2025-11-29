namespace MisFinanzas.Domain.DTOs
{
    // DTO para mensajes del chat con el asistente de IA
    public class AIMessageDto
    {
        public string Role { get; set; } = string.Empty; // "user" o "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
