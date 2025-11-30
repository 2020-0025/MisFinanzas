namespace MisFinanzas.Domain.DTOs;

// Resultado de la ejecución de un comando
public class CommandResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? ErrorDetails { get; set; }
}

