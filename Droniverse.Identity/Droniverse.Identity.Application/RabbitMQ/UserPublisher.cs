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

    private IModel? _channel;
    private IConnection? _connection;
    private bool _isConnected;

    private readonly ConnectionFactory _factory;

    public UserPublisher(IConfiguration configuration, ILogger<UserPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;

        string hostName = _configuration["RabbitMQ_HostName"] ?? "localhost";
        string userName = _configuration["RabbitMQ_UserName"] ?? "guest";
        string password = _configuration["RabbitMQ_Password"] ?? "guest";
        string port = _configuration["RabbitMQ_Port"] ?? "5672";

        _factory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = int.Parse(port),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(1),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _isConnected = false;
    }

    private void EnsureConnection()
    {
        if (_isConnected && _connection != null && _channel != null)
            return;

        try
        {
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _isConnected = true;

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ not available");
            _isConnected = false;
            _connection = null;
            _channel = null;
        }
    }

    public void Publish<T>(string exchange, string routingKey, T message)
    {
        EnsureConnection();

        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ not connected, skip publish");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed");
        }
    }

    public void Publish<T>(Dictionary<string, object> headers, T message)
    {
        EnsureConnection();

        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ not connected, skip publish");
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed");
        }
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
            _logger.LogError(ex, "Dispose failed");
        }
    }
}