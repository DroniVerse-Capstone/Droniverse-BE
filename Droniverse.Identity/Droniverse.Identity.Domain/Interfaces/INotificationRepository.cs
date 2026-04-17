using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetUnsentNotificationsAsync();
    Task<IEnumerable<Notification>> GetPendingNotificationsByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAsReadByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxRetries = 3);
}
