using RabbitMQ.Client;

namespace RapidMQ.Contracts;

public interface IChannelFactory
{
    public Task<IModel> CreateChannel(IConnection connection);
}