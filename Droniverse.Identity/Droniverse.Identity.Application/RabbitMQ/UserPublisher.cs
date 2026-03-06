using Droniverse.Shared.Messages;
using Droniverse.Shared.Messages.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Droniverse.Identity.Application.RabbitMQ;

internal class UserPublisher : IPublisher, IDisposable, IUserPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<UserPublisher> _logger;
    private readonly IModel? _channel;
    private readonly IConnection? _connection;
    private readonly bool _isConnected;

    public UserPublisher(IConfiguration configuration, ILogger<UserPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _isConnected = false;

        try
        {
            string hostName = _configuration["RabbitMQ_HostName"] ?? "localhost";
            string userName = _configuration["RabbitMQ_UserName"] ?? "guest";
            string password = _configuration["RabbitMQ_Password"] ?? "guest";
            string port = _configuration["RabbitMQ_Port"] ?? "5672";

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = int.Parse(port),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _isConnected = true;
            
            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ. The application will continue without message broker functionality.");
            _isConnected = false;
            _connection = null;
            _channel = null;
        }
    }

    public void Publish<T>(string exchange, string routingKey, T message)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ is not connected. Message publishing skipped for exchange: {Exchange}, routingKey: {RoutingKey}", exchange, routingKey);
            return;
        }

        try
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);
            
            _channel.ExchangeDeclare(
                exchange: exchange,
                type: ExchangeType.Direct,
                durable: true);

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: messageBodyInBytes);

            _logger.LogDebug("Message published successfully to exchange: {Exchange}, routingKey: {RoutingKey}", exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}", exchange, routingKey);
        }
    }

    public void Publish<T>(Dictionary<string, object> headers, T message)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ is not connected. Message publishing skipped with headers: {Headers}", string.Join(", ", headers.Keys));
            return;
        }

        try
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

            string exchangeName = _configuration["RabbitMQ_Users_Exchange"] ?? "users.exchange";
            
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Headers,
                durable: true);

            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.Headers = headers;
            
            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: string.Empty,
                basicProperties: basicProperties,
                body: messageBodyInBytes);

            _logger.LogDebug("Message published successfully to exchange: {Exchange} with headers", exchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ with headers: {Headers}", string.Join(", ", headers.Keys));
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
            
            if (_isConnected)
            {
                _logger.LogInformation("RabbitMQ connection disposed successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}

