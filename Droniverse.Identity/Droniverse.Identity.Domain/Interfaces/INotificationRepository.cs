using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId);
    Task<PaginationResult<IEnumerable<NotificationResponse>>> GetNotificationsByUserPagedAsync(
        Guid userId,
        NotificationStatus? status,
        DateTime? sentAt,
        int pageIndex,
        int pageSize);
    Task<IEnumerable<Notification>> GetUnsentNotificationsAsync();
    Task<IEnumerable<Notification>> GetPendingNotificationsByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<bool> MarkAsReadByUserAsync(Guid userId);
    Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxRetries = 3);
}
