using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;

namespace RapidMQ.Internals;

public static class RetryPolicyProvider
{
    public static AsyncRetryPolicy GetConnectionRecoveryRetryPolicy(RetryConfiguration retryConfiguration,
        Action<string, int, TimeSpan>  onRetry = null, CancellationToken cancellationToken = default)
    {
        return Policy.Handle<BrokerUnreachableException>()
            .Or<AlreadyClosedException>()
            .Or<ConnectFailureException>()
            .Or<OperationInterruptedException>()
            .WaitAndRetryForeverAsync(sleepDuration =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var exponentialDelay = TimeSpan.FromMilliseconds(retryConfiguration.InitialMillisecondsRetry *
                                                                     Math.Pow(2, sleepDuration - 1));
                    var maxDelay = TimeSpan.FromMilliseconds(retryConfiguration.MaxMillisecondsDelay);
                    return exponentialDelay < maxDelay ? exponentialDelay : maxDelay;
                },
                (exception, retryCount, span) =>
                {
                    const string exceptionType = nameof(exception);
                    onRetry?.Invoke($"{exceptionType} - {exception.Message}", retryCount, span);
                });
    }
}