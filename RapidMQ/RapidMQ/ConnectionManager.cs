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
    private Func<Task>? OnConnectionRecovery { get; set; }

    protected virtual IConnection CreateConnectionInstance(Uri uri)
    {
        return new ConnectionFactory
        {
            Uri = uri,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
        }.CreateConnection();
    }

    public virtual async Task<IConnection> ConnectAsync(Uri connectionUri,
        ConnectionManagerConfig connectionManagerConfig)
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
                Task.Run(async () => { await OnConnectionShutdownEventHandler(args); })
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            _logger.LogError(t.Exception, "OnConnectionShutdownEventHandler failed!");
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
            onRetry: (exception, span, attemptNr) =>
            {
                isReconnectionAttempted = true;
                var onReconnectRetryEventHandler = OnReconnectRetryEventHandler;
                if (onReconnectRetryEventHandler != null)
                {
                    Task.Run(async () => await onReconnectRetryEventHandler.Invoke(exception, attemptNr, span))
                        .ContinueWith(
                            t =>
                            {
                                if (t.IsFaulted)
                                    _logger.LogError(t.Exception, "OnReconnectRetryEventHandler failed!");
                            });
                }

                _logger.LogError(
                    "Retrying to connect to the RabbitMQ server after: {AttemptNr} attempt(s), timespan (ms): {Span}! - Exception: {ExceptionMessage}",
                    attemptNr, span.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    exception.InnerException?.Message ?? exception.Message);
            });

        var connection = await policy.ExecuteAsync(_ =>
            Task.FromResult(CreateConnectionInstance(uri)), CancellationToken.None);

        if (!isReconnectionAttempted) return connection;

        var onConnectionRecovery = OnConnectionRecovery;
        if (onConnectionRecovery != null)
        {
            Task.Run(async () => await onConnectionRecovery.Invoke())
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogError(t.Exception, "OnConnectionRecovery failed!");
                });
        }

        return connection;
    }
}