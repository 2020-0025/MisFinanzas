using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services;

/// Servicio de fondo que genera automáticamente notificaciones para gastos fijos
/// Se ejecuta cada día a las 12:00 AM (medianoche)
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    //private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Verificar cada 24 horas

    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // TESTING: Verificar cada minuto (cambiar a 24 horas en producción)

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

        // Esperar hasta la próxima medianoche para la primera ejecución

        // TESTING: Ejecutar inmediatamente sin esperar medianoche
        // await WaitUntilMidnight(stoppingToken);
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

            // Esperar 24 horas hasta la próxima ejecución
            // await Task.Delay(_checkInterval, stoppingToken);

            // Esperar el intervalo configurado (1 minuto en testing, 24 horas en producción)
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