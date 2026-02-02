namespace Droniverse.Shared.Messages.User;

public interface IUserConsumer
{
    void Consummer<T>(string exchange, string routingKey, T message);
    void Publish<T>(Dictionary<string, object> headers, T message);
}

