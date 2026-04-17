using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Enums;

namespace Droniverse.Identity.Application.IService;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? relatedEntityId = null);
    Task<Notification> SendAndCreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string userEmail, string? relatedEntityId = null);
    Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetPendingNotificationsByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAsReadByUserAsync(Guid userId);
}
