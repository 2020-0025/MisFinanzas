using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces;

// Interfaz para el servicio de asistente de IA
// Proporciona funcionalidades de chat y análisis financiero usando Google Gemini
public interface IAIAssistantService
{
    // Envía un mensaje al asistente de IA y obtiene una respuesta
    /// </summary>
    /// <param name="userMessage">Mensaje del usuario</param>
    /// <param name="userId">ID del usuario para contexto personalizado</param>
    /// <param name="conversationHistory">Historial de conversación opcional</param>
    /// <returns>Respuesta del asistente de IA</returns>
    Task<AIMessageDto> SendMessageAsync(string userMessage, string userId, List<AIMessageDto>? conversationHistory = null);

    // Verifica si el servicio de IA está disponible (API key configurada)
    // <returns>True si el servicio está disponible</returns>
    bool IsAvailable();
}
