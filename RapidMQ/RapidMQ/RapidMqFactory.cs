using Microsoft.Extensions.Logging;
using RapidMQ.Contracts;

namespace RapidMQ;

public class RapidMqFactory : IRapidMqFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<IRapidMq> _logger;

    public RapidMqFactory(IConnectionManager connectionManager,
        ILogger<IRapidMq> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<RapidMq> CreateAsync(Uri connectionUri)
    {
        var connection = await _connectionManager.ConnectAsync(connectionUri);
        return new RapidMq(connection, _logger);
    }
}