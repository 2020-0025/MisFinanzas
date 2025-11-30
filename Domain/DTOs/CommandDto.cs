using MisFinanzas.Domain.Enums;

namespace MisFinanzas.Domain.DTOs;

// Representa un comando detectado en el mensaje del usuario
public class CommandDto
{
    public CommandType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool RequiresConfirmation { get; set; } = true;
    public string ConfirmationMessage { get; set; } = string.Empty;

    // --- NUEVAS PROPIEDADES ---
    public bool CreateCategoryIfMissing { get; set; } // ¿Debo crearla si no existe?
    public string? SuggestedIcon { get; set; }        // ¿Qué ícono sugirió la IA?
}


