using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Enums;
using Droniverse.Shared.Messages.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Droniverse.Identity.Application.RabbitMQ;

public class OrderNotificationConsumer : IDisposable
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderNotificationConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IModel? _channel;
    private IConnection? _connection;

    public OrderNotificationConsumer(
        INotificationService notificationService,
        IConfiguration configuration,
        ILogger<OrderNotificationConsumer> logger)
    {
        _notificationService = notificationService;
        _configuration = configuration;
        _logger = logger;
    }

    public void Start()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ_HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ_UserName"] ?? "guest",
                Password = _configuration["RabbitMQ_Password"] ?? "guest",
                Port = int.Parse(_configuration["RabbitMQ_Port"] ?? "5672"),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            string exchangeName = "order.notification.exchange";
            string queueName = "identity.notification.queue";

            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true);

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "order.created");

            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "payment.successful");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    string routingKey = ea.RoutingKey;

                    _logger.LogInformation($"Message received with routing key: {routingKey}");

                    if (routingKey == "order.created")
                    {
                        var orderMsg = JsonSerializer.Deserialize<OrderCreatedNotificationMessage>(message);
                        if (orderMsg != null)
                        {
                            await HandleOrderCreatedAsync(orderMsg);
                        }
                    }
                    else if (routingKey == "payment.successful")
                    {
                        var paymentMsg = JsonSerializer.Deserialize<PaymentSuccessfulNotificationMessage>(message);
                        if (paymentMsg != null)
                        {
                            await HandlePaymentSuccessfulAsync(paymentMsg);
                        }
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true); // Requeue on error
                }
            };

            _channel.BasicQos(0, 1, false); // Process one message at a time
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("Order notification consumer started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting consumer");
        }
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedNotificationMessage message)
    {
        try
        {
            _logger.LogInformation($"Processing order created notification for user {message.UserId}");

            // Tạo notification pending cho order mới
            await _notificationService.SendAndCreateNotificationAsync(
                message.UserId,
                "Đơn hàng đang chờ thanh toán",
                $"Bạn vừa tạo đơn hàng #{message.OrderId}. Tổng tiền: {FormatCurrency(message.Total)}. Vui lòng thực hiện thanh toán để hoàn tất đơn hàng.",
                NotificationType.EMAIL,
                message.UserEmail,
                message.OrderId.ToString()
            );

            _logger.LogInformation($"Order created notification sent for order {message.OrderId}");
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
            _logger.LogInformation($"Processing payment successful notification for user {message.UserId}");

            // Mark previous pending notification as read (nếu có)
            // Tạo notification mới về thanh toán thành công
            await _notificationService.SendAndCreateNotificationAsync(
                message.UserId,
                "Thanh toán thành công",
                $"Đơn hàng #{message.OrderId} của bạn đã được thanh toán thành công. Số tiền: {FormatCurrency(message.Amount)}. Cảm ơn bạn đã mua hàng!",
                NotificationType.EMAIL,
                message.UserEmail,
                message.OrderId.ToString()
            );

            _logger.LogInformation($"Payment successful notification sent for order {message.OrderId}");
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
