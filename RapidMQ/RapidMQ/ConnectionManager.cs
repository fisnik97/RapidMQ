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

    public async Task<IConnection> ConnectAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig)
    {
        if (connectionUri == null)
            throw new ArgumentNullException(nameof(connectionUri));

        ValidateConfiguration(connectionManagerConfig);

        OnReconnectRetryEventHandler = connectionManagerConfig.OnReconnectRetryEventHandler;
        OnConnectionShutdownEventHandler = connectionManagerConfig.OnConnectionShutdownEventHandler;

        var connection = await ConnectToBroker(connectionUri,
            connectionManagerConfig.MaxConnectionRetries,
            connectionManagerConfig.DelayBetweenRetries);

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

    private async Task<IConnection> ConnectToBroker(Uri uri, int maxRetries, TimeSpan delay)
    {
        var policy = PolicyProvider.GetBackOffRetryPolicy<BrokerUnreachableException>(
            maxRetries,
            delay,
            onRetry: ((exception, span, attemptNr) =>
            {
                var  onReconnectRetryEventHandler = OnReconnectRetryEventHandler;

                try
                {
                    onReconnectRetryEventHandler?.Invoke(exception, attemptNr, span);
                }
                catch (Exception e)
                {
                    _logger.LogError("OnReconnectRetryEventHandler - {Message}", e.Message);
                }

                Console.WriteLine(
                    $"Could not connect to the RabbitMQ server after {attemptNr}(s) attempt, timespan: {span}! - Exception: {exception.InnerException?.Message ?? exception.Message}");

                _logger.LogError(
                    "Could not connect to the RabbitMQ server after {AttemptNr}(s) attempt, timespan: {Span}! - Exception: {ExceptionMessage}",
                    attemptNr, span, exception.InnerException?.Message ?? exception.Message);
            }));

        var connection = await policy.ExecuteAsync(_ =>
            Task.FromResult(new
                ConnectionFactory
                {
                    Uri = uri,
                    RequestedHeartbeat = TimeSpan.FromSeconds(30),
                }.CreateConnection()), CancellationToken.None);

        return connection;
    }

    private static void ValidateConfiguration(ConnectionManagerConfig config)
    {
        // Apply rules for connection manager config
    }
}