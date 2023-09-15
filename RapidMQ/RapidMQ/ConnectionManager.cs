using System.Globalization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RapidMQ.Contracts;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class ConnectionManager : IConnectionManager
{
    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Func<ShutdownEventArgs, Task>? OnConnectionShutdownEventHandler { get; set; }
    private Func<Exception, int, TimeSpan, Task>? OnReconnectRetryEventHandler { get; set; }
    private Action? OnConnectionRecovery { get; set; }

    public async Task<IConnection> ConnectAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig)
    {
        if (connectionUri == null)
            throw new ArgumentNullException(nameof(connectionUri), "Connection URI cannot be null!");

        OnReconnectRetryEventHandler = connectionManagerConfig.OnReconnectRetryEventHandler;
        OnConnectionShutdownEventHandler = connectionManagerConfig.OnConnectionShutdownEventHandler;
        OnConnectionRecovery = connectionManagerConfig.OnConnectionRecovery;

        var connection = await ConnectToBroker(connectionUri,
            connectionManagerConfig.MaxConnectionRetries,
            connectionManagerConfig.DelayBetweenRetries,
            connectionManagerConfig.ExponentialBackoffRetry
        );

        if (OnConnectionShutdownEventHandler != null)
            connection.ConnectionShutdown += (_, args) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        OnConnectionShutdownEventHandler(args);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e,
                            "Exception occurred in OnConnectionShutdownEventHandler");
                    }
                });
            };

        return connection;
    }

    private async Task<IConnection> ConnectToBroker(Uri uri, int maxRetries, TimeSpan delay,
        bool exponentialBackoffRetry)
    {
        var isReconnectionAttempted = false;
        var policy = PolicyProvider.GetAsyncRetryPolicy<BrokerUnreachableException>(
            new RetryConfiguration(maxRetries, (int)delay.TotalMilliseconds, exponentialBackoffRetry),
            onRetry: ((exception, span, attemptNr) =>
            {
                isReconnectionAttempted = true;
                var onReconnectRetryEventHandler = OnReconnectRetryEventHandler;
                try
                {
                    Task.Run(() => onReconnectRetryEventHandler?.Invoke(exception, attemptNr, span));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "OnReconnectRetryEventHandler failed!");
                }

                _logger.LogError(
                    "Could not connect to the RabbitMQ server after {AttemptNr}(s) attempt, timespan (ms): {Span}! - Exception: {ExceptionMessage}",
                    attemptNr, span.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    exception.InnerException?.Message ?? exception.Message);
            }));

        var connection = await policy.ExecuteAsync(_ =>
            Task.FromResult(new
                ConnectionFactory
                {
                    Uri = uri,
                    RequestedHeartbeat = TimeSpan.FromSeconds(30),
                }.CreateConnection()), CancellationToken.None);

        if (!isReconnectionAttempted) return connection;

        try
        {
            await Task.Run(() => OnConnectionRecovery?.Invoke());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OnConnectionRecovery failed!");
        }

        return connection;
    }
}