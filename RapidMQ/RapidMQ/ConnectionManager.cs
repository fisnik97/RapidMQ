using System.Globalization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RapidMQ.Contracts;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class ConnectionManager : IConnectionManager
{
    private readonly ILogger<ConnectionManager> _logger;
    private CancellationToken _cancellationToken;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Func<ShutdownEventArgs, Task> OnConnectionShutdownEventHandler { get; set; }
    private Func<Task> OnConnection { get; set; }

    /// <summary>
    /// Method to connect to the RabbitMQ broker using a capped exponential backoff retry policy.
    /// </summary>
    /// <param name="connectionUri"></param>
    /// <param name="connectionManagerConfig"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task<IConnection> ConnectAsync(Uri connectionUri,
        ConnectionManagerConfig connectionManagerConfig, CancellationToken cancellationToken = default)
    {
        if (connectionUri == null)
            throw new ArgumentNullException(nameof(connectionUri), "Connection URI cannot be null!");

        _cancellationToken = cancellationToken;

        OnConnectionShutdownEventHandler = connectionManagerConfig.OnConnectionShutdownEventHandler;
        OnConnection = connectionManagerConfig.OnConnection;

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

    /// <summary>
    /// Connects to the RabbitMQ broker using a capped exponential backoff retry policy.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="maxMillisecondsDelay"></param>
    /// <param name="initialMillisecondsRetry"></param>
    /// <returns></returns>
    private async Task<IConnection> ConnectToBroker(Uri uri, int maxMillisecondsDelay, int initialMillisecondsRetry)
    {
        var policy = RetryPolicyProvider.GetConnectionRecoveryRetryPolicy(
            new RetryConfiguration(maxMillisecondsDelay, initialMillisecondsRetry),
            onRetry: OnReconnectRetry,
            cancellationToken: _cancellationToken);

        var connection = await policy.ExecuteAsync(_ =>
            Task.FromResult(CreateConnectionInstance(uri)), _cancellationToken);

        if (OnConnection == null) return connection;

        Task.Run(async () => await OnConnection.Invoke(), _cancellationToken)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                    _logger.LogError(t.Exception, "OnConnection");

                if (t.IsCanceled)
                    _logger.LogWarning("OnConnection canceled!");
            }, _cancellationToken);

        return connection;
    }

    protected virtual IConnection CreateConnectionInstance(Uri uri)
    {
        return new ConnectionFactory
        {
            Uri = uri,
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        }.CreateConnection();
    }

    private void OnReconnectRetry(string exceptionMessage, int attemptNr, TimeSpan span)
    {
        _logger.LogError(
            "Retrying to connect to the RabbitMQ server after: {AttemptNr} attempt(s), timespan (ms): {Span}! - Exception: {ExceptionMessage}",
            attemptNr, span.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), exceptionMessage);
    }
}