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
    private readonly IClock _clock;

    public OrderNotificationPublisher(IConfiguration configuration, ILogger<OrderNotificationPublisher> logger, IClock clock)
    {
        _configuration = configuration;
        _logger = logger;
        _clock = clock;
    }

    private void EnsureConnection()
    {
        if (_connection != null && _channel != null)
            return;

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

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port
            };
            _connection = factory.CreateConnection();
            _connection.ConnectionBlocked += (_, args) =>
                _logger.LogWarning("RabbitMQ connection blocked: {Reason}", args.Reason);
            _connection.ConnectionUnblocked += (_, _) =>
                _logger.LogInformation("RabbitMQ connection unblocked");
            _connection.CallbackException += (_, args) =>
                _logger.LogError(args.Exception, "RabbitMQ connection callback exception");
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
            basicProperties.Timestamp = new AmqpTimestamp(new DateTimeOffset(_clock.Now).ToUnixTimeSeconds());

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
            throw;
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
            basicProperties.Timestamp = basicProperties.Timestamp = new AmqpTimestamp(new DateTimeOffset(_clock.Now).ToUnixTimeSeconds());

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
