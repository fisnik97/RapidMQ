using RabbitMQ.Client;

namespace RapidMQ.Models;

public class ConnectionManagerConfig
{
    /// <summary>
    /// Threshold for the number of connection retries before giving up.
    /// </summary>
    public int MaxConnectionRetries { get; init; }

    /// <summary>
    /// Delay between connection retries. Initial delay if ExponentialBackoffRetry is set to true.
    /// </summary>
    public TimeSpan DelayBetweenRetries { get; init; }

    /// <summary>
    /// Enables exponential backoff for connection retries.
    /// </summary>
    public bool ExponentialBackoffRetry { get; init; }

    /// <summary>
    /// Custom client delegate invoked when the connection is shutdown.
    /// </summary>
    public Func<ShutdownEventArgs, Task>? OnConnectionShutdownEventHandler { get; init; }

    /// <summary>
    /// Custom client delegate invoked when re-trying to connect to the broker.
    /// </summary>
    public Func<Exception, int, TimeSpan, Task>? OnReconnectRetryEventHandler { get; init; }

    /// <summary>
    /// Represents a delegate that client can provide to be invoked when the connection is recovered.
    /// </summary>
    public Func<Task>? OnConnectionRecovery { get; init; }

    public ConnectionManagerConfig(int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry)
    {
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
    }

    public ConnectionManagerConfig(int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler)
    {
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
    }

    public ConnectionManagerConfig(int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler,
        Func<Exception, int, TimeSpan, Task>? onReconnectRetryEventHandler)
    {
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnReconnectRetryEventHandler = onReconnectRetryEventHandler;
    }

    public ConnectionManagerConfig(int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler,
        Func<Exception, int, TimeSpan, Task>? onReconnectRetryEventHandler, Func<Task>? onConnectionRecovery)
    {
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnReconnectRetryEventHandler = onReconnectRetryEventHandler;
        OnConnectionRecovery = onConnectionRecovery;
    }
}