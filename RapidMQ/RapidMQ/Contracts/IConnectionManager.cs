using RabbitMQ.Client;

namespace RapidMQ.Contracts;

public interface IConnectionManager
{
    Task<IConnection> ConnectAsync(Uri uri);
}