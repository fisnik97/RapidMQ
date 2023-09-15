using Polly;
using Polly.Retry;

namespace RapidMQ.Internals;

public static class PolicyProvider
{
    public static AsyncRetryPolicy GetAsyncRetryPolicy<TException>(RetryConfiguration retryConfiguration,
        Action<TException, TimeSpan, int>? onRetry = null) where TException : Exception
    {
        return Policy.Handle<TException>()
            .WaitAndRetryAsync(retryConfiguration.MaxRetries,
                retryAttempt => retryConfiguration.ExponentialBackoffRetry
                    ? TimeSpan.FromMilliseconds(retryConfiguration.MillisecondsBetweenRetries *
                                                Math.Pow(2, retryAttempt - 1))
                    : TimeSpan.FromMilliseconds(retryConfiguration.MillisecondsBetweenRetries),
                (exception, timeSpan, retryCount, context) =>
                {
                    onRetry?.Invoke((TException)exception, timeSpan, retryCount);
                }
            );
    }
}