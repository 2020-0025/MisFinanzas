using MisFinanzas.Domain.DTOs;

namespace MisFinanzas.Infrastructure.Interfaces;

// Servicio para ejecutar comandos detectados
public interface ICommandExecutorService
{
    // Ejecuta un comando y retorna el resultado
    Task<CommandResultDto> ExecuteCommandAsync(CommandDto command, string userId);
}
