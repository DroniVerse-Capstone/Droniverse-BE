using Droniverse.Shared.Messages.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Droniverse.Community.Application.RabbitMQ;

internal class OrderNotificationPublisher : IOrderNotificationPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderNotificationPublisher> _logger;
    private IModel? _channel;
    private IConnection? _connection;

    public OrderNotificationPublisher(IConfiguration configuration, ILogger<OrderNotificationPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private void EnsureConnection()
    {
        if (_connection != null && _channel != null)
            return;

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ_HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ_UserName"] ?? "guest",
                Password = _configuration["RabbitMQ_Password"] ?? "guest",
                Port = int.Parse(_configuration["RabbitMQ_Port"] ?? "5672")
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
            throw;
        }
    }

    public void Publish<T>(string exchange, string routingKey, T message)
    {
        try
        {
            EnsureConnection();

            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

            _channel!.ExchangeDeclare(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true);

            var basicProperties = _channel!.CreateBasicProperties();
            basicProperties.Persistent = true;
            basicProperties.ContentType = "application/json";
            basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel!.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: basicProperties,
                body: messageBodyInBytes);

            _logger.LogInformation($"Message published to {exchange} with routing key {routingKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed");
        }
    }

    public void Publish<T>(Dictionary<string, object> headers, T message)
    {
        try
        {
            EnsureConnection();

            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

            string exchangeName = _configuration["RabbitMQ_Users_Exchange"] ?? "users.exchange";

            _channel!.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Headers,
                durable: true);

            var basicProperties = _channel!.CreateBasicProperties();
            basicProperties.Headers = headers;
            basicProperties.Persistent = true;
            basicProperties.ContentType = "application/json";
            basicProperties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel!.BasicPublish(
                exchange: exchangeName,
                routingKey: string.Empty,
                basicProperties: basicProperties,
                body: messageBodyInBytes);

            _logger.LogInformation($"Message published to {exchangeName} with headers");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed");
        }
    }

    public async Task PublishOrderCreatedAsync(OrderCreatedNotificationMessage message)
    {
        await Task.Run(() =>
        {
            Publish(
                exchange: "order.notification.exchange",
                routingKey: "order.created",
                message: message
            );
        });
    }

    public async Task PublishPaymentSuccessfulAsync(PaymentSuccessfulNotificationMessage message)
    {
        await Task.Run(() =>
        {
            Publish(
                exchange: "order.notification.exchange",
                routingKey: "payment.successful",
                message: message
            );
        });
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dispose failed");
        }
    }
}
