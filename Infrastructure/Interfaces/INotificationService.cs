using MisFinanzas.Domain.Entities;

namespace MisFinanzas.Infrastructure.Interfaces
{
    public interface INotificationService
    {
        // Obtener notificaciones
        Task<List<Notification>> GetUnreadNotificationsByUserAsync(string userId);
        Task<List<Notification>> GetAllNotificationsByUserAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);

        // Marcar como leído
        Task<bool> MarkAsReadAsync(int notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);

        // Generar notificaciones automáticas
        Task GenerateNotificationsForFixedExpensesAsync();

        // Generar notificación inmediata para una categoría específica
        Task GenerateNotificationForCategoryAsync(int categoryId);

        // Limpiar notificaciones antiguas
        Task CleanOldNotificationsAsync(int daysOld = 60);

        Task<bool> DeleteNotificationAsync(int notificationId, string userId);

        // Eliminar notificaciones futuras de una categoría (para actualización)
        Task DeleteFutureNotificationsByCategoryAsync(int categoryId, string userId);
    }
}
