namespace Droniverse.Shared.Messages.Notification;

public record PaymentSuccessfulNotificationMessage(
    Guid UserId,
    Guid OrderId,
    string UserEmail,
    string UserName,
    decimal Amount,
    DateTime PaidAt
);
