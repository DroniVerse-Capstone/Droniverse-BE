using Droniverse.Shared.Messages;
using Droniverse.Shared.Messages.User;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Droniverse.Identity.Application.RabbitMQ;

internal class UserPublisher : IPublisher, IDisposable, IUserPublisher
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    public UserPublisher(IConfiguration configuration)
    {
        _configuration = configuration;

        string hostName = _configuration["RabbitMQ_HostName"];
        string userName = _configuration["RabbitMQ_UserName"];
        string password = _configuration["RabbitMQ_Password"];
        string port = _configuration["RabbitMQ_Port"];

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = int.Parse(port)
        }; 
        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    public void Publish<T>(string exchange, string routingKey, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);
        _channel.ExchangeDeclare(
            exchange: exchange, 
            type: ExchangeType.Direct, 
            durable: true);
        //Publish the message to the exchange
        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: null,
            body: messageBodyInBytes);
    }

    public void Publish<T>(Dictionary<string, object> headers, T message)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

        string exchangeName = _configuration["RabbitMQ_Users_Exchange"];
        _channel.ExchangeDeclare(
            exchange: exchangeName, 
            type: ExchangeType.Headers, 
            durable: true);

        //Publish the message to the exchange
        var basicProperties = _channel.CreateBasicProperties();
        basicProperties.Headers = headers;
        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: string.Empty,
            basicProperties: basicProperties, // custom when use Header exchange
            body: messageBodyInBytes);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

