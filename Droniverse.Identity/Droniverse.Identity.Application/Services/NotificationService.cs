using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Logging;

namespace Droniverse.Identity.Application.Services;

internal class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string? relatedEntityId = null)
    {
        try
        {
            var notification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                UserID = userId,
                Title = title,
                Message = message,
                Type = type,
                Status = NotificationStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                SentAt = null,
                RetryCount = 0,
                RelatedEntityID = relatedEntityId,
                ErrorMessage = null
            };

            _logger.LogInformation("Creating notification entity for user {UserId} title={Title} relatedEntityId={RelatedEntityId}", userId, title, relatedEntityId);
            await _unitOfWork.Notifications.Add(notification);
            var saved = await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("Saved notification {NotificationId} to DB (result={SaveResult})", notification.NotificationID, saved);
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetNotificationsByUserAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetPendingNotificationsByUserAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetPendingNotificationsByUserAsync(userId);
    }

    public async Task<Notification> SendAndCreateNotificationAsync(Guid userId, string title, string message, NotificationType type, string userEmail, string? relatedEntityId = null)
    {
        var notification = new Notification
        {
            NotificationID = Guid.NewGuid(),
            UserID = userId,
            Title = title,
            Message = message,
            Type = type,
            Status = NotificationStatus.PENDING,
            CreatedAt = DateTime.UtcNow,
            SentAt = null,
            RetryCount = 0,
            RelatedEntityID = relatedEntityId,
            ErrorMessage = null
        };

        try
        {
            _logger.LogInformation("Preparing to send notification to user {UserId} via {Type} (email={Email})", userId, type, userEmail);
            // Gửi notification ngay (real-time)
            if (type == NotificationType.EMAIL)
            {
                await _emailService.SendEmailAsync(userEmail, title, message);
                notification.Status = NotificationStatus.SENT;
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Notification sent successfully to {Email}", userEmail);
            }
            else
            {
                // Cho các type khác (SMS, PUSH, IN_APP) có thể handle sau
                notification.Status = NotificationStatus.SENT;
                notification.SentAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            notification.Status = NotificationStatus.FAILED;
            notification.ErrorMessage = ex.Message;
            notification.RetryCount = 1;
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
        }

        // Lưu notification vào DB
        try
        {
            _logger.LogInformation("Adding notification entity to DB for user {UserId} (NotificationId={NotificationId})", userId, notification.NotificationID);
            await _unitOfWork.Notifications.Add(notification);
            var saved = await _unitOfWork.SaveChangeAsync();
            _logger.LogInformation("Saved notification {NotificationId} to DB (result={SaveResult})", notification.NotificationID, saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving notification to DB for user {UserId} NotificationId={NotificationId}", userId, notification.NotificationID);
            throw;
        }

        return notification;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var result = await _unitOfWork.Notifications.MarkAsReadAsync(notificationId);
        if (result)
        {
            await _unitOfWork.SaveChangeAsync();
        }
        return result;
    }

    public async Task<bool> MarkAsReadByUserAsync(Guid userId)
    {
        var result = await _unitOfWork.Notifications.MarkAsReadByUserAsync(userId);
        if (result)
        {
            await _unitOfWork.SaveChangeAsync();
        }
        return result;
    }
}
