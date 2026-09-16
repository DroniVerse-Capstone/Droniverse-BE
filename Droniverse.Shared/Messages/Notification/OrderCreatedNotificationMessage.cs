namespace Droniverse.Shared.Messages.Notification;

public record OrderCreatedNotificationMessage(
    Guid UserId,
    Guid OrderId,
    string UserEmail,
    string UserName,
    decimal Total,
    DateTime CreatedAt
);
