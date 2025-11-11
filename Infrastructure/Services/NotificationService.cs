using Microsoft.EntityFrameworkCore;
using MisFinanzas.Domain.Entities;
using MisFinanzas.Domain.Enums;
using MisFinanzas.Infrastructure.Data;
using MisFinanzas.Infrastructure.Interfaces;

namespace MisFinanzas.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUnreadNotificationsByUserAsync(string userId)
        {
            return await _context.Notifications
                .Include(n => n.Category)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderBy(n => n.DueDate)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllNotificationsByUserAsync(string userId)
        {
            return await _context.Notifications
                .Include(n => n.Category)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// Genera notificaciones para TODAS las categorías con gastos fijos
        /// Llamado por el servicio de fondo diariamente
        public async Task GenerateNotificationsForFixedExpensesAsync()
        {
            var today = DateTime.Now.Date;
            var daysToNotify = 3; // Notificar 3 días antes

            // Obtener todas las categorías con gastos fijos activos
            var fixedExpenseCategories = await _context.Categories
                .Where(c => c.IsFixedExpense && c.DayOfMonth.HasValue)
                .ToListAsync();

            foreach (var category in fixedExpenseCategories)
            {
                await GenerateNotificationForCategoryIfNeededAsync(category, today, daysToNotify);
            }

            await _context.SaveChangesAsync();
        }

        /// Genera notificación INMEDIATA para una categoría específica (llamado al crear/editar)
        public async Task GenerateNotificationForCategoryAsync(int categoryId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.IsFixedExpense && c.DayOfMonth.HasValue);

            if (category == null)
                return;

            var today = DateTime.Now.Date;
            var daysToNotify = 3;

            await GenerateNotificationForCategoryIfNeededAsync(category, today, daysToNotify);
            await _context.SaveChangesAsync();
        }

        /// Lógica centralizada para generar notificación de una categoría
        /// Verifica:
        /// 1. Si ya existe notificación para este mes
        /// 2. Si ya se registró un pago de esta categoría este mes
        /// 3. Si estamos dentro de los 3 días antes de la fecha límite
        private async Task GenerateNotificationForCategoryIfNeededAsync(Category category, DateTime today, int daysToNotify)
        {
            // Calcular la fecha de vencimiento del mes actual
            var currentMonth = today.Month;
            var currentYear = today.Year;
            var dayOfMonth = category.DayOfMonth!.Value;

            // Ajustar si el día es mayor que los días del mes
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            if (dayOfMonth > daysInMonth)
                dayOfMonth = daysInMonth;

            var dueDateCurrentMonth = new DateTime(currentYear, currentMonth, dayOfMonth);

            // VERIFICACIÓN 1: ¿Ya existe una notificación para este mes?
            var existingNotification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.CategoryId == category.CategoryId &&
                    n.UserId == category.UserId &&
                    n.DueDate.Month == currentMonth &&
                    n.DueDate.Year == currentYear);

            if (existingNotification != null)
            {
                // Ya existe notificación para este mes, no crear duplicado
                return;
            }

            // VERIFICACIÓN 2: ¿Ya se registró un pago de esta categoría este mes?
            var startOfMonth = new DateTime(currentYear, currentMonth, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var hasPaymentThisMonth = await _context.ExpensesIncomes
                .AnyAsync(ei =>
                    ei.CategoryId == category.CategoryId &&
                    ei.UserId == category.UserId &&
                    ei.Type == TransactionType.Expense &&
                    ei.Date >= startOfMonth &&
                    ei.Date <= endOfMonth);

            if (hasPaymentThisMonth)
            {
                // Ya pagó este mes, no generar notificación
                return;
            }

            // DECIDIR: ¿Generar para mes actual (vencida) o próximo mes (futura)?
            DateTime dueDate;

            if (dueDateCurrentMonth < today)
            {
                // CASO 1: La fecha ya pasó este mes → Generar notificación VENCIDA para este mes
                dueDate = dueDateCurrentMonth;
                Console.WriteLine($"⚠️ Generando notificación VENCIDA para '{category.Title}' - vencía el {dueDate:dd/MM/yyyy}");
            }
            else
            {
                // CASO 2: La fecha aún no llega → Generar si faltan 3 días o menos
                var notificationDate = dueDateCurrentMonth.AddDays(-daysToNotify);

                if (today >= notificationDate && today <= dueDateCurrentMonth)
                {
                    // Estamos dentro de la ventana de 3 días antes
                    dueDate = dueDateCurrentMonth;
                    Console.WriteLine($"🔔 Generando notificación para '{category.Title}' - vence el {dueDate:dd/MM/yyyy}");
                }
                else
                {
                    // Aún no es momento de notificar (faltan más de 3 días)
                    return;
                }
            }

            // Crear nueva notificación
            var notification = new Notification
            {
                CategoryId = category.CategoryId,
                UserId = category.UserId,
                NotificationDate = today,
                DueDate = dueDate,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
        }

        public async Task CleanOldNotificationsAsync(int daysOld = 60)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysOld);

            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

                if (notification == null)
                    return false;

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting notification: {ex.Message}");
                return false;
            }
        }

        /// Elimina todas las notificaciones del mes actual de una categoría
        /// Incluye notificaciones vencidas, de hoy y futuras del mes en curso
        /// Útil cuando se actualiza la fecha de vencimiento de un gasto fijo
        public async Task DeleteFutureNotificationsByCategoryAsync(int categoryId, string userId)
        {
            try
            {
                var today = DateTime.Now.Date;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Eliminar TODAS las notificaciones del mes actual (vencidas, hoy y futuras)
                var currentMonthNotifications = await _context.Notifications
                    .Where(n => n.CategoryId == categoryId &&
                               n.UserId == userId &&
                               n.DueDate >= startOfMonth &&
                               n.DueDate <= endOfMonth)
                    .ToListAsync();

                if (currentMonthNotifications.Any())
                {
                    _context.Notifications.RemoveRange(currentMonthNotifications);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"🗑️ Eliminadas {currentMonthNotifications.Count} notificación(es) del mes actual de la categoría {categoryId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Eliminando Notificación(es): {ex.Message}");
            }
        }
    }
}