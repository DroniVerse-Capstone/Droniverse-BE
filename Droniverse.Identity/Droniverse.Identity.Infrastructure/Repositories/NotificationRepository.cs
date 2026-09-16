using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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

    public async Task<PaginationResult<IEnumerable<NotificationResponse>>> GetNotificationsByUserPagedAsync(
        Guid userId,
        NotificationStatus? status,
        DateTime? sentAt,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Notification> query = _dbSet
            .AsNoTracking()
            .Where(n => n.UserID == userId);

        if (status.HasValue)
        {
            query = query.Where(n => n.Status == status.Value);
        }

        if (sentAt.HasValue)
        {
            var startDate = sentAt.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(n => n.SentAt.HasValue && n.SentAt.Value >= startDate && n.SentAt.Value < endDate);
        }

        query = query.OrderByDescending(n => n.CreatedAt);

        var totalRecords = await query.CountAsync();
        var notifications = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResponse
            {
                NotificationID = n.NotificationID,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                Status = n.Status.ToString(),
                CreatedAt = n.CreatedAt,
                SentAt = n.SentAt,
                ErrorMessage = n.ErrorMessage
            })
            .ToListAsync();

        return new PaginationResult<IEnumerable<NotificationResponse>>(
            notifications,
            totalRecords,
            pageIndex,
            pageSize);
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
