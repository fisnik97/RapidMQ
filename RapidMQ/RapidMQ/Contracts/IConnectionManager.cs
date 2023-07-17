using RabbitMQ.Client;

namespace RapidMQ.Contracts;

public interface IConnectionManager
{
    public Func<ShutdownEventArgs, Task>? OnConnectionDrop { get; set; }
    Task<IConnection> ConnectAsync(Uri uri);
}