using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Shared.Messages.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Droniverse.Identity.Application.RabbitMQ;

public class OrderNotificationConsumer : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IModel? _channel;
    private IConnection? _connection;
    private EventingBasicConsumer? _consumer;

    public OrderNotificationConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<OrderNotificationConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public void Consume()
    {
        try
        {
            var hostName = _configuration["RabbitMQ_HostName"] ?? "localhost";
            var userName = _configuration["RabbitMQ_UserName"] ?? "guest";
            var password = _configuration["RabbitMQ_Password"] ?? "guest";
            var portStr = _configuration["RabbitMQ_Port"] ?? "5672";

            // Handle cases where port is embedded in a full connection string (e.g., "tcp://10.109.19.235:15672")
            int port = 5672;
            if (Uri.TryCreate($"amqp://{portStr}", UriKind.Absolute, out var uri) && uri.Port > 0)
            {
                port = uri.Port;
                if (string.IsNullOrEmpty(hostName) || hostName == "localhost")
                {
                    hostName = uri.Host;
                }
            }
            else if (int.TryParse(portStr, out int parsedPort))
            {
                port = parsedPort;
            }

            _logger.LogInformation("🔧 RabbitMQ Config - Host: {HostName}, Port: {Port}, User: {UserName}", 
                hostName, port, userName);

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            string exchangeName = "order.notification.exchange";
            string queueName = "identity.notification.queue";

            _logger.LogInformation("📌 Declaring exchange: {ExchangeName}", exchangeName);
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _logger.LogInformation("📌 Declaring queue: {QueueName}", queueName);
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _logger.LogInformation("📌 Binding queue to exchange with routing key: order.created");
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "order.created");

            _logger.LogInformation("📌 Binding queue to exchange with routing key: payment.successful");
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "payment.successful");

            _logger.LogInformation("📌 Creating EventingBasicConsumer (synchronous)...");
            _consumer = new EventingBasicConsumer(_channel);

            _consumer.Received += async (sender, args) =>
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                string routingKey = args.RoutingKey;

                _logger.LogInformation("✅ MESSAGE RECEIVED - RoutingKey: {RoutingKey}", routingKey);
                _logger.LogInformation("📦 Payload: {Message}", message);

                try
                {
                    if (routingKey == "order.created")
                    {
                        _logger.LogInformation("Processing order.created...");
                        try
                        {
                            var orderMsg = JsonSerializer.Deserialize<OrderCreatedNotificationMessage>(message);
                            if (orderMsg != null)
                            {
                                await HandleOrderCreatedAsync(orderMsg);
                                _logger.LogInformation("Order created notification processed for order {OrderId}", orderMsg.OrderId);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to deserialize OrderCreatedNotificationMessage. Payload: {Payload}", message);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "JSON deserialization error for order.created. Payload: {Payload}", message);
                        }
                    }
                    else if (routingKey == "payment.successful")
                    {
                        _logger.LogInformation("Processing payment.successful...");
                        try
                        {
                            var paymentMsg = JsonSerializer.Deserialize<PaymentSuccessfulNotificationMessage>(message);
                            if (paymentMsg != null)
                            {
                                await HandlePaymentSuccessfulAsync(paymentMsg);
                                _logger.LogInformation("Payment successful notification processed for order {OrderId}", paymentMsg.OrderId);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to deserialize PaymentSuccessfulNotificationMessage. Payload: {Payload}", message);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, " JSON deserialization error for payment.successful. Payload: {Payload}", message);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message with routing key {RoutingKey}", routingKey);
                }
            };

            _logger.LogInformation("Starting BasicConsume on queue: {QueueName}", queueName);
            string consumerTag = _channel.BasicConsume(queue: queueName, consumer: _consumer, autoAck: true);
            _logger.LogInformation("BasicConsume started with consumerTag: {ConsumerTag}", consumerTag);
            _logger.LogInformation("Order notification consumer started successfully - waiting for messages...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting order notification consumer");
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedNotificationMessage message)
    {
        try
        {
            _logger.LogInformation($"🔄 Processing order created notification for user {message.UserId}");

            // Create a scope to resolve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Add timeout to prevent hanging
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    // Tạo notification pending cho order mới
                    try
                    {
                        var created = await notificationService.SendAndCreateNotificationAsync(
                            message.UserId,
                            "Đơn hàng đang chờ thanh toán",
                            $"Bạn vừa tạo đơn hàng #{message.OrderId}. Tổng tiền: {FormatCurrency(message.Total)}. Vui lòng thực hiện thanh toán để hoàn tất đơn hàng.",
                            NotificationType.EMAIL,
                            message.UserEmail,
                            message.OrderId.ToString()
                        );

                        if (created != null)
                        {
                            _logger.LogInformation("Notification created: Id={NotificationId}, Status={Status}, User={UserId}, OrderId={OrderId}",
                                created.NotificationID, created.Status, message.UserId, message.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning("SendAndCreateNotificationAsync returned null for order {OrderId}", message.OrderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SendAndCreateNotificationAsync failed for order {OrderId}, user {UserId}", message.OrderId, message.UserId);
                        throw;
                    }
                }
            }

            _logger.LogInformation($"✅ Order created notification processed successfully for order {message.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling order created notification for order {message.OrderId}");
            throw;
        }
    }

    private async Task HandlePaymentSuccessfulAsync(PaymentSuccessfulNotificationMessage message)
    {
        try
        {
            _logger.LogInformation($"🔄 Processing payment successful notification for user {message.UserId}");

            // Create a scope to resolve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Add timeout to prevent hanging
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    // Mark previous pending notification as read (nếu có)
                    // Tạo notification mới về thanh toán thành công
                    try
                    {
                        var created = await notificationService.SendAndCreateNotificationAsync(
                            message.UserId,
                            "Thanh toán thành công",
                            $"Đơn hàng #{message.OrderId} của bạn đã được thanh toán thành công. Số tiền: {FormatCurrency(message.Amount)}. Cảm ơn bạn đã mua hàng!",
                            NotificationType.EMAIL,
                            message.UserEmail,
                            message.OrderId.ToString()
                        );

                        if (created != null)
                        {
                            _logger.LogInformation("Notification created: Id={NotificationId}, Status={Status}, User={UserId}, OrderId={OrderId}",
                                created.NotificationID, created.Status, message.UserId, message.OrderId);
                        }
                        else
                        {
                            _logger.LogWarning("SendAndCreateNotificationAsync returned null for payment order {OrderId}", message.OrderId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "SendAndCreateNotificationAsync failed for payment order {OrderId}, user {UserId}", message.OrderId, message.UserId);
                        throw;
                    }
                }
            }

            _logger.LogInformation($"✅ Payment successful notification processed successfully for order {message.OrderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling payment successful notification for order {message.OrderId}");
            throw;
        }
    }

    private string FormatCurrency(decimal amount)
    {
        return $"{amount:N0} VNĐ";
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing consumer");
        }
    }
}
