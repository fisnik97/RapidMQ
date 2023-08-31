using RabbitMQ.Client;

namespace RapidMQ.Models;

public class ConnectionManagerConfig
{
    public Uri Uri { get; init; }
    public int MaxConnectionRetries { get; init; }
    public TimeSpan DelayBetweenRetries { get; init; }
    public bool ExponentialBackoffRetry { get; init; }
    public Func<ShutdownEventArgs, Task>? OnConnectionShutdownEventHandler {  get; init; }
    public Func<Exception, int, TimeSpan, Task>? OnReconnectRetryEventHandler { get; init; }

    public ConnectionManagerConfig(Uri uri, int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry)
    {
        Uri = uri;
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
    }

    public ConnectionManagerConfig(Uri uri, int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler)
    {
        Uri = uri;
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
    }

    public ConnectionManagerConfig(Uri uri, int maxConnectionRetries, TimeSpan delayBetweenRetries,
        bool exponentialBackoffRetry,
        Func<ShutdownEventArgs, Task>? onConnectionShutdownEventHandler,
        Func<Exception, int, TimeSpan, Task>? onReconnectRetryEventHandler)
    {
        Uri = uri;
        MaxConnectionRetries = maxConnectionRetries;
        DelayBetweenRetries = delayBetweenRetries;
        ExponentialBackoffRetry = exponentialBackoffRetry;
        OnConnectionShutdownEventHandler = onConnectionShutdownEventHandler;
        OnReconnectRetryEventHandler = onReconnectRetryEventHandler;
    }
}