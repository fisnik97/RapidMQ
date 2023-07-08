using RapidMQ.Contracts;

namespace RapidMQ;

public class RapidMqFactory : IRapidMqFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly IChannelFactory _channelFactory;

    public RapidMqFactory(IConnectionManager connectionManager, IChannelFactory channelFactory)
    {
        _connectionManager = connectionManager;
        _channelFactory = channelFactory;
    }

    public async Task<RapidMq> CreateAsync(Uri connectionUri)
    {
        var connection = await _connectionManager.ConnectAsync(connectionUri);
        var channel = await _channelFactory.CreateChannel(connection);
        return new RapidMq(connection, channel);
    }
}