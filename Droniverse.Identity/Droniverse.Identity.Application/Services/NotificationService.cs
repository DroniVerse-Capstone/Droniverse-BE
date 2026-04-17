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

            await _unitOfWork.Notifications.Add(notification);
            await _unitOfWork.SaveChangeAsync();

            _logger.LogInformation($"Notification created successfully for user {userId}");
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating notification for user {userId}: {ex.Message}");
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
            // Gửi notification ngay (real-time)
            if (type == NotificationType.EMAIL)
            {
                await _emailService.SendEmailAsync(userEmail, title, message);
                notification.Status = NotificationStatus.SENT;
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation($"Notification sent successfully to {userEmail}");
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
            _logger.LogError($"Failed to send notification to user {userId}: {ex.Message}");
        }

        // Lưu notification vào DB
        await _unitOfWork.Notifications.Add(notification);
        await _unitOfWork.SaveChangeAsync();

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
