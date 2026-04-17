using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers;

[ApiController]
[Route("identity/notitications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy tất cả notification của user hiện tại
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyNotifications()
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User not authenticated");

            var notifications = await _notificationService.GetNotificationsByUserAsync(userId);
            return Ok(new
            {
                success = true,
                data = notifications,
                message = "Get notifications successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting notifications: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Lấy số lượng notification chưa đọc
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User not authenticated");

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new
            {
                success = true,
                data = count,
                message = "Get unread count successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting unread count: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Đánh dấu notification là đã đọc
    /// </summary>
    [HttpPut("{notificationId}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        try
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId);
            if (!result)
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found"
                });

            return Ok(new
            {
                success = true,
                message = "Notification marked as read"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking notification as read: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Đánh dấu tất cả notification của user là đã đọc
    /// </summary>
    [HttpPut("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (userId == Guid.Empty)
                return Unauthorized("User not authenticated");

            var result = await _notificationService.MarkAsReadByUserAsync(userId);
            return Ok(new
            {
                success = true,
                result = result,
                message = result ? "All notifications marked as read" : "No unread notifications"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error marking all notifications as read: {ex.Message}");
            return StatusCode(500, new
            {
                success = false,
                message = "Internal server error"
            });
        }
    }
}
