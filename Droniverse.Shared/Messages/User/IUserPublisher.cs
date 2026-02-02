namespace Droniverse.Shared.Messages.User;

public interface IUserPublisher
{
    void Publish<T>(string exchange, string routingKey, T message);
    void Publish<T>(Dictionary<string, object> headers, T message);
}

