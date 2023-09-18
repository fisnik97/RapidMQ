using RabbitMQ.Client;

namespace RapidMQ.Models;

public class ConnectionManagerConfig
{
    /// <summary>
    /// Threshold for the number of connection retries before giving up.
    /// </summary>
    public int MaxMillisecondsDelay { get; init; }

    /// <summary>
    /// Delay between connection retries. Initial delay if ExponentialBackoffRetry is set to true.
    /// </summary>
    public int InitialMillisecondsRetry { get; init; }

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

    public ConnectionManagerConfig(int maxMillisecondsDelay, int initialMillisecondsRetry)
    {
        MaxMillisecondsDelay = maxMillisecondsDelay;
        InitialMillisecondsRetry = initialMillisecondsRetry;
    }

    public ConnectionManagerConfig(int maxMillisecondsDelay, int initialMillisecondsRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler)
    {
        MaxMillisecondsDelay = maxMillisecondsDelay;
        InitialMillisecondsRetry = initialMillisecondsRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
    }

    public ConnectionManagerConfig(int maxMillisecondsDelay, int initialMillisecondsRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler,
        Func<Exception, int, TimeSpan, Task>? onReconnectRetryEventHandler)
    {
        MaxMillisecondsDelay = maxMillisecondsDelay;
        InitialMillisecondsRetry = initialMillisecondsRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnReconnectRetryEventHandler = onReconnectRetryEventHandler;
    }

    public ConnectionManagerConfig(int maxMillisecondsDelay, int initialMillisecondsRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler,
        Func<Exception, int, TimeSpan, Task>? onReconnectRetryEventHandler, Func<Task>? onConnectionRecovery)
    {
        MaxMillisecondsDelay = maxMillisecondsDelay;
        InitialMillisecondsRetry = initialMillisecondsRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnReconnectRetryEventHandler = onReconnectRetryEventHandler;
        OnConnectionRecovery = onConnectionRecovery;
    }
}