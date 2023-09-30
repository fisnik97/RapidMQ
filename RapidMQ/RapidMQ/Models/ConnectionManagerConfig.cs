using RabbitMQ.Client;

namespace RapidMQ.Models;

public class ConnectionManagerConfig
{
    /// <summary>
    /// Max increase in delay between connection retries 
    /// </summary>
    public int MaxMillisecondsDelay { get; init; }

    /// <summary>
    /// Initial or Starting delay between connection retries until MaxMillisecondsDelay is reached exponentially 
    /// </summary>
    public int InitialMillisecondsRetry { get; init; }

    /// <summary>
    /// Custom client delegate invoked when the connection between the client and the broker is shutdown.
    /// </summary>
    public Func<ShutdownEventArgs, Task>? OnConnectionShutdownEventHandler { get; init; }

    /// <summary>
    /// Represents a delegate that client can provide to be invoked when the connection is made with the broker.
    /// </summary>
    public Func<Task>? OnConnection { get; init; }

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
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler, Func<Task>? onConnection)
    {
        MaxMillisecondsDelay = maxMillisecondsDelay;
        InitialMillisecondsRetry = initialMillisecondsRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnConnection = onConnection;
    }
}