using Droniverse.Shared.Messages;

namespace Droniverse.Shared.Messages.Notification;

public interface IOrderNotificationPublisher : IPublisher
{
    Task PublishOrderCreatedAsync(OrderCreatedNotificationMessage message);
    Task PublishPaymentSuccessfulAsync(PaymentSuccessfulNotificationMessage message);
}
