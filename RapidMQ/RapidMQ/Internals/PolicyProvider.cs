using Polly;
using Polly.Retry;

namespace RapidMQ.Internals;

public static class PolicyProvider
{
    public static AsyncRetryPolicy GetCappedForeverRetryPolicy<TException>(RetryConfiguration retryConfiguration,
        Action<TException, int, TimeSpan>? onRetry = null, CancellationToken cancellationToken = default)
        where TException : Exception
    {
        return Policy.Handle<TException>()
            .WaitAndRetryForeverAsync(sleepDuration =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var exponentialDelay = TimeSpan.FromMilliseconds(retryConfiguration.InitialMillisecondsRetry *
                                                                 Math.Pow(2, sleepDuration - 1));
                var maxDelay = TimeSpan.FromMilliseconds(retryConfiguration.MaxMillisecondsDelay);

                return exponentialDelay < maxDelay ? exponentialDelay : maxDelay;
            }, (exception, retryCount, span) => { onRetry?.Invoke((TException)exception, retryCount, span); });
    }
}