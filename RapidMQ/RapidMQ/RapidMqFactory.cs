using System.Text.Json;
using Microsoft.Extensions.Logging;
using RapidMQ.Contracts;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidMqFactory : IRapidMqFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly Logger<IRapidMq> _logger;

    public RapidMqFactory(IConnectionManager connectionManager,
        Logger<IRapidMq> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<RapidMq> CreateAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var connection = await _connectionManager.ConnectAsync(connectionUri, connectionManagerConfig);

        return new RapidMq(connection, _logger, _connectionManager, connectionManagerConfig, connectionUri,
            jsonSerializerOptions);
    }
}