using Polly;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;

namespace RapidMQ.Internals;

public static class RetryPolicyProvider
{
    /// <summary>
    /// Capped exponential backoff retry policy for connection recovery.
    /// </summary>
    /// <param name="retryConfiguration"></param>
    /// <param name="onRetry"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static AsyncRetryPolicy GetCappedExponentialRetryPolicy(RetryConfiguration retryConfiguration,
        Action<string, int, TimeSpan> onRetry = null, CancellationToken cancellationToken = default)
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
                    
                    if (exponentialDelay > TimeSpan.MaxValue - TimeSpan.FromMinutes(1))
                        exponentialDelay = TimeSpan.MaxValue - TimeSpan.FromMinutes(1);
                    else if (exponentialDelay > maxDelay)
                        exponentialDelay = maxDelay; 

                    return exponentialDelay;
                },
                (exception, retryCount, span) =>
                {
                    const string exceptionType = nameof(exception);
                    onRetry?.Invoke($"{exceptionType} - {exception.Message}", retryCount, span);
                });
    }
}