using RabbitMQ.Client;
using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IConnectionManager
{
    /// <summary>
    /// Creates a new connection to the RabbitMQ broker.
    /// </summary>
    /// <param name="connectionUri">RabbitMq broker URI </param>
    /// <param name="connectionManagerConfig">ConnectionManager configurations </param>
    /// <param name="cancellationToken"></param>
    /// <returns>RabbitMQ.Client.IConnection</returns>
    Task<IConnection> ConnectAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig,
        CancellationToken cancellationToken = default);
}