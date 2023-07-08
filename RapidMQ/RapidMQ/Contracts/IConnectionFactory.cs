using RabbitMQ.Client;

namespace RapidMQ;

public interface IConnectionManager
{
    Task<IConnection> ConnectAsync(Uri uri);
}