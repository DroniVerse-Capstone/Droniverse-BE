namespace Droniverse.Community.Application.IRabbitMQ;

public interface IUserNameUpdateConsumer
{
    void Consume();
    void Dispose();
}

