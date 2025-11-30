using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces;

// Servicio para detectar y parsear comandos en mensajes de usuario
public interface ICommandParserService
{
    // Detecta si un mensaje contiene un comando ejecutable
    Task<CommandDto?> ParseCommandAsync(string message, string userId);
}
