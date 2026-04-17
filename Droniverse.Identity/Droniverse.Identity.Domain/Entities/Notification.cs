using Droniverse.Identity.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Droniverse.Identity.Domain.Entities;

public class Notification
{
    public Guid NotificationID { get; set; }
    public Guid UserID { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RetryCount { get; set; }
    public string? RelatedEntityID { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Account Account { get; set; }
}
