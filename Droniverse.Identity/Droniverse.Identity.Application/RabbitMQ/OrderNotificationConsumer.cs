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

namespace Droniverse.Identity.Application.RabbitMQ;

public class OrderNotificationConsumer : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IModel? _channel;
    private IConnection? _connection;

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
            var port = int.Parse(_configuration["RabbitMQ_Port"] ?? "5672");

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
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
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
                        _logger.LogInformation("🔄 Processing order.created...");
                        var orderMsg = JsonSerializer.Deserialize<OrderCreatedNotificationMessage>(message);
                        if (orderMsg != null)
                        {
                            await HandleOrderCreatedAsync(orderMsg);
                            _logger.LogInformation("✅ Order created notification processed for order {OrderId}", orderMsg.OrderId);
                        }
                    }
                    else if (routingKey == "payment.successful")
                    {
                        _logger.LogInformation("🔄 Processing payment.successful...");
                        var paymentMsg = JsonSerializer.Deserialize<PaymentSuccessfulNotificationMessage>(message);
                        if (paymentMsg != null)
                        {
                            await HandlePaymentSuccessfulAsync(paymentMsg);
                            _logger.LogInformation("✅ Payment successful notification processed for order {OrderId}", paymentMsg.OrderId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing message with routing key {RoutingKey}", routingKey);
                }
            };

            _logger.LogInformation("🔄 Starting BasicConsume on queue: {QueueName}", queueName);
            _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
            _logger.LogInformation("✅ Order notification consumer started successfully - waiting for messages...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting order notification consumer");
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
                    await notificationService.SendAndCreateNotificationAsync(
                        message.UserId,
                        "Đơn hàng đang chờ thanh toán",
                        $"Bạn vừa tạo đơn hàng #{message.OrderId}. Tổng tiền: {FormatCurrency(message.Total)}. Vui lòng thực hiện thanh toán để hoàn tất đơn hàng.",
                        NotificationType.EMAIL,
                        message.UserEmail,
                        message.OrderId.ToString()
                    );
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
                    await notificationService.SendAndCreateNotificationAsync(
                        message.UserId,
                        "Thanh toán thành công",
                        $"Đơn hàng #{message.OrderId} của bạn đã được thanh toán thành công. Số tiền: {FormatCurrency(message.Amount)}. Cảm ơn bạn đã mua hàng!",
                        NotificationType.EMAIL,
                        message.UserEmail,
                        message.OrderId.ToString()
                    );
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
