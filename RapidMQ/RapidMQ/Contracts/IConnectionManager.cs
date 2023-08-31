using RabbitMQ.Client;
using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IConnectionManager
{
    Task<IConnection> ConnectAsync(ConnectionManagerConfig connectionManagerConfig);
}