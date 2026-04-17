using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Identity.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(IdentityDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(n => n.UserID == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnsentNotificationsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(n => n.Status == Domain.Enums.NotificationStatus.PENDING)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsByUserAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(n => n.UserID == userId && n.Status == Domain.Enums.NotificationStatus.PENDING)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _dbSet
            .AsNoTracking()
            .CountAsync(n => n.UserID == userId && n.Status != Domain.Enums.NotificationStatus.READ);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _dbSet.FirstOrDefaultAsync(n => n.NotificationID == notificationId);
        if (notification == null)
            return false;

        notification.Status = Domain.Enums.NotificationStatus.READ;
        return true;
    }

    public async Task<bool> MarkAsReadByUserAsync(Guid userId)
    {
        var notifications = await _dbSet
            .Where(n => n.UserID == userId && n.Status != Domain.Enums.NotificationStatus.READ)
            .ToListAsync();

        if (notifications.Count == 0)
            return false;

        foreach (var notification in notifications)
        {
            notification.Status = Domain.Enums.NotificationStatus.READ;
        }

        return true;
    }

    public async Task<IEnumerable<Notification>> GetFailedNotificationsAsync(int maxRetries = 3)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(n => n.Status == Domain.Enums.NotificationStatus.FAILED && n.RetryCount < maxRetries)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();
    }
}
