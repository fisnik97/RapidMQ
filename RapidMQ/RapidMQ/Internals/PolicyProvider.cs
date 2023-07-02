using Polly;
using Polly.Retry;

namespace RapidMQ.Internals;

public static class PolicyProvider
{
    public static AsyncRetryPolicy GetBackOffRetryPolicy(int maxRetries = 5, int secondsDelay = 2,
        Action<Exception, TimeSpan, int>? onRetry = default)
    {
        return Policy.Handle<Exception>()
            .WaitAndRetryAsync(maxRetries, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(secondsDelay, retryAttempt)),
                (exception, timeSpan, retryCount, context) => { onRetry?.Invoke(exception, timeSpan, retryCount); }
            );
    }

    public static AsyncRetryPolicy GetLinearRetryPolicy(int maxRetries = 5, int secondsDelay = 2,
        Action<Exception, TimeSpan, int>? onRetry = default)
    {
        return Policy.Handle<Exception>()
            .WaitAndRetryAsync(maxRetries, retryAttempt =>
                    TimeSpan.FromSeconds(secondsDelay),
                (exception, timeSpan, retryCount, context) => { onRetry?.Invoke(exception, timeSpan, retryCount); }
            );
    }
}