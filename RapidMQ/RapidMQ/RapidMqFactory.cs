using System.Text.Json;
using Microsoft.Extensions.Logging;
using RapidMQ.Contracts;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidMqFactory : IRapidMqFactory
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<IRapidMq> _logger;
    private readonly ITelemetryService _telemetryService;

    public RapidMqFactory(IConnectionManager connectionManager,
        ILogger<IRapidMq> logger, ITelemetryService telemetryService)
    {
        _connectionManager = connectionManager;
        _logger = logger;
        _telemetryService = telemetryService;
    }

    public async Task<RapidMq> CreateAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig,
        CancellationToken cancellationToken = default,
        JsonSerializerOptions jsonSerializerOptions = null)
    {
        var connection =
            await _connectionManager.ConnectAsync(connectionUri, connectionManagerConfig, cancellationToken);

        return new RapidMq(
            connection,
            _logger,
            _connectionManager,
            connectionManagerConfig,
            connectionUri,
            _telemetryService,
            jsonSerializerOptions,
            cancellationToken
        );
    }
}