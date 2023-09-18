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
    private CancellationToken _cancellationToken;
    private bool _isReconnectionAttempted;

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
            AutomaticRecoveryEnabled = true,
        }.CreateConnection();
    }

    public virtual async Task<IConnection> ConnectAsync(Uri connectionUri,
        ConnectionManagerConfig connectionManagerConfig, CancellationToken cancellationToken = default)
    {
        if (connectionUri == null)
            throw new ArgumentNullException(nameof(connectionUri), "Connection URI cannot be null!");

        _cancellationToken = cancellationToken;
        _isReconnectionAttempted = false;

        OnReconnectRetryEventHandler = connectionManagerConfig.OnReconnectRetryEventHandler;
        OnConnectionShutdownEventHandler = connectionManagerConfig.OnConnectionShutdownEventHandler;
        OnConnectionRecovery = connectionManagerConfig.OnConnectionRecovery;

        var connection = await ConnectToBroker(connectionUri,
            connectionManagerConfig.MaxMillisecondsDelay,
            connectionManagerConfig.InitialMillisecondsRetry
        );

        if (OnConnectionShutdownEventHandler != null)
        {
            connection.ConnectionShutdown += (_, args) =>
            {
                Task.Run(async () => { await OnConnectionShutdownEventHandler(args); }, _cancellationToken)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            _logger.LogError(t.Exception, "OnConnectionShutdownEventHandler failed!");

                        if (t.IsCanceled)
                            _logger.LogWarning("OnConnectionShutdownEventHandler canceled!");
                    }, _cancellationToken);
            };
        }

        return connection;
    }

    private async Task<IConnection> ConnectToBroker(Uri uri, int maxMillisecondsDelay, int initialMillisecondsRetry)
    {
        var policy = PolicyProvider.GetCappedForeverRetryPolicy<BrokerUnreachableException>(
            new RetryConfiguration(maxMillisecondsDelay, initialMillisecondsRetry),
            onRetry: OnReconnectRetry,
            cancellationToken: _cancellationToken);

        var connection = await policy.ExecuteAsync(_ =>
            Task.FromResult(CreateConnectionInstance(uri)), _cancellationToken);

        if (!_isReconnectionAttempted) return connection;

        var onConnectionRecovery = OnConnectionRecovery;
        if (onConnectionRecovery != null)
        {
            Task.Run(async () => await onConnectionRecovery.Invoke(), _cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogError(t.Exception, "OnConnectionRecovery failed!");

                    if (t.IsCanceled)
                        _logger.LogWarning("OnConnectionRecovery canceled!");
                }, _cancellationToken);
        }

        return connection;
    }

    private void OnReconnectRetry(Exception exception, int attemptNr, TimeSpan span)
    {
        _isReconnectionAttempted = true;
        var onReconnectRetryEventHandler = OnReconnectRetryEventHandler;
        if (onReconnectRetryEventHandler != null)
        {
            Task.Run(async () => await onReconnectRetryEventHandler.Invoke(exception, attemptNr, span),
                    _cancellationToken)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted) _logger.LogError(t.Exception, "OnReconnectRetryEventHandler failed!");

                    if (t.IsCanceled) _logger.LogWarning("OnReconnectRetryEventHandler canceled!");
                }, _cancellationToken);
        }

        _logger.LogError(
            "Retrying to connect to the RabbitMQ server after: {AttemptNr} attempt(s), timespan (ms): {Span}! - Exception: {ExceptionMessage}",
            attemptNr, span.TotalMilliseconds, exception.InnerException?.Message ?? exception.Message);
    }
}