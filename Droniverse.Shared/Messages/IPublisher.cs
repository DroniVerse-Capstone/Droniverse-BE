namespace Droniverse.Shared.Messages;

public interface IPublisher
{
    void Publish<T>(string exchange, string routingKey, T message); // Direct exchange
    void Publish<T>(Dictionary<string, object> headers, T message); // Header exchange
}

