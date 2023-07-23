using Microsoft.Extensions.Logging;
using RapidMQ.Contracts;

namespace RapidMQ;

public class RapidMqFactory : IRapidMqFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly IChannelFactory _channelFactory;
    private readonly ILogger<IRapidMq> _logger;

    public RapidMqFactory(IConnectionManager connectionManager, IChannelFactory channelFactory,
        ILogger<IRapidMq> logger)
    {
        _connectionManager = connectionManager;
        _channelFactory = channelFactory;
        _logger = logger;
    }

    public async Task<IRapidMq> CreateAsync(Uri connectionUri)
    {
        var connection = await _connectionManager.ConnectAsync(connectionUri);
        return new RapidMq(connection, _channelFactory, _logger);
    }
}