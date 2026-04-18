using Droniverse.Community.Application.IRabbitMQ;
using Droniverse.Shared.Messages.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Droniverse.Community.Application.RabbitMQ;

public class UserNameUpdateConsumer : IDisposable, IUserNameUpdateConsumer
{
    private readonly ILogger<UserNameUpdateConsumer> _logger;
    private readonly IConfiguration _configuration;
    private IModel? _channel;
    private IConnection? _connection;

    public UserNameUpdateConsumer(
        IConfiguration configuration,
        ILogger<UserNameUpdateConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Consume()
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

            string exchangeName = _configuration["RabbitMQ_Users_Exchange"] ?? "users.exchange";
            string queueName = "community.user.update.queue";

            _logger.LogInformation("📌 Declaring exchange: {ExchangeName}", exchangeName);
            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Headers,
                durable: true);

            _logger.LogInformation("📌 Declaring queue: {QueueName}", queueName);
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind with header x-match: all and event-type: username-updated
            var arguments = new Dictionary<string, object>
            {
                { "x-match", "all" },
                { "event-type", "username-updated" }
            };

            _logger.LogInformation("📌 Binding queue to exchange with header filter");
            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: "",
                arguments: arguments);

            _logger.LogInformation("📌 Creating EventingBasicConsumer (synchronous)...");
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("✅ USERNAME UPDATE MESSAGE RECEIVED");
                _logger.LogInformation("📦 Payload: {Message}", message);

                try
                {
                    var userUpdateMsg = JsonSerializer.Deserialize<UserNameUpdateMessage>(message);
                    if (userUpdateMsg != null)
                    {
                        await HandleUserNameUpdateAsync(userUpdateMsg);
                        _logger.LogInformation("✅ User name update processed for user {UserId}", userUpdateMsg.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing username update message");
                }
            };

            _logger.LogInformation("🔄 Starting BasicConsume on queue: {QueueName}", queueName);
            _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
            _logger.LogInformation("✅ User name update consumer started successfully - waiting for messages...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting user name update consumer");
        }
    }

    private async Task HandleUserNameUpdateAsync(UserNameUpdateMessage message)
    {
        try
        {
            _logger.LogInformation($"Processing user name update for user {message.UserId}: new name = {message.NewUserName}");

            // TODO: Implement user cache update or database sync
            // This could update:
            // 1. Cache (Redis)
            // 2. Local user store
            // 3. Search index
            // For now, just log the event

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling user name update for user {message.UserId}");
            throw;
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
            _logger.LogError(ex, "Error disposing consumer");
        }
    }
}

