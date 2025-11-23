using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services;
/// Servicio de fondo que genera automáticamente notificaciones para gastos fijos
///
/// ========== CONFIGURACIÓN DE MODO ==========
///
/// MODO TESTING (Para demostración/presentación):
/// - Línea 36: Mantener TimeSpan.FromMinutes(1) activo
/// - Línea 33: Mantener TimeSpan.FromHours(24) comentado
/// - Línea 51: Mantener await WaitUntilMidnight() comentado
/// - Línea 54: Mantener mensaje "MODO TESTING" activo
///
/// MODO PRODUCCIÓN (Después de la presentación):
/// - Línea 36: Comentar TimeSpan.FromMinutes(1)
/// - Línea 33: Descomentar TimeSpan.FromHours(24)
/// - Línea 51: Descomentar await WaitUntilMidnight()
/// - Línea 54: Comentar mensaje "MODO TESTING"
///
/// ==========================================
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;

    // MODO PRODUCCIÓN: Descomentar esta línea después de la presentación
    // private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    // MODO TESTING: Comentar esta línea después de la presentación
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔔 NotificationBackgroundService iniciado");

        // MODO PRODUCCIÓN: Descomentar esta línea después de la presentación
        // await WaitUntilMidnight(stoppingToken);

        // MODO TESTING: Comentar esta línea después de la presentación
        _logger.LogInformation("⚡ MODO TESTING: Ejecutando inmediatamente cada minuto");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🔔 Generando notificaciones automáticas diarias...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.GenerateNotificationsForFixedExpensesAsync();
                }

                _logger.LogInformation("✅ Notificaciones generadas exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al generar notificaciones automáticas");
            }

            // Esperar el intervalo configurado (1 minuto en modo testing, 24 horas en modo producción)
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    /// Espera hasta la próxima medianoche (12:00 AM)
    private async Task WaitUntilMidnight(CancellationToken stoppingToken)
    {
        var now = DateTime.Now;
        var nextMidnight = now.Date.AddDays(1); // Próxima medianoche
        var timeUntilMidnight = nextMidnight - now;

        _logger.LogInformation($"⏰ Primera ejecución en {timeUntilMidnight.TotalHours:F1} horas (próxima medianoche)");

        if (timeUntilMidnight.TotalMinutes > 0)
        {
            await Task.Delay(timeUntilMidnight, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🛑 NotificationBackgroundService detenido");
        await base.StopAsync(stoppingToken);
    }
}