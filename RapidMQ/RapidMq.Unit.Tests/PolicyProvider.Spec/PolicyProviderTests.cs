namespace RapidMq.Unit.Tests.PolicyProvider.Spec;

public class PolicyProviderTests
{
    /*
    [Fact(DisplayName = "Test retry attempt count with backoff disabled")]
    public async Task Test_RetryCount_WithoutExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, false);
        var exceptionCount = 0;

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider.GetCappedForeverRetryPolicy<CustomException>(config, (ex, ts, count)
                    => exceptionCount++)
                .ExecuteAsync(() => throw new CustomException());
        });

        Assert.Equal(3, exceptionCount);
    }

    [Fact(DisplayName = "Test retry attempt count with backoff enabled")]
    public async Task Test_RetryCount_WithExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, true);
        var exceptionCount = 0;

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider.GetCappedForeverRetryPolicy<CustomException>(config, (ex, ts, count)
                    => exceptionCount++)
                .ExecuteAsync(() => throw new CustomException());
        });

        Assert.Equal(3, exceptionCount);
    }

    [Fact(DisplayName = "Test retry delay with backoff enabled")]
    public async Task Test_RetryDelay_WithExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, true);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider.GetCappedForeverRetryPolicy<CustomException>(config)
                .ExecuteAsync(() => throw new CustomException());
        });

        stopwatch.Stop();
        // 3 retries with exponential backoff: 100ms + 200ms + 400ms = 700ms
        Assert.True(stopwatch.ElapsedMilliseconds >= 700);
    }

    [Fact(DisplayName = "Test retry delay with backoff disabled")]
    public async Task Test_RetryDelay_WithoutExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, false);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider.GetCappedForeverRetryPolicy<CustomException>(config)
                .ExecuteAsync(() => throw new CustomException());
        });

        stopwatch.Stop();
        // 3 retries without exponential backoff: 100ms + 100ms + 100ms = 300ms
        Assert.True(stopwatch.ElapsedMilliseconds >= 300);
    }

    [Fact(DisplayName = "Test onRetry-Callback invocation with backoff disabled")]
    public async Task Test_OnRetryCallbackExecution_WithoutExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, false);
        var callbackExecutionCount = 0;

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider
                .GetCappedForeverRetryPolicy<CustomException>(config, (ex, ts, count)
                    => callbackExecutionCount++)
                .ExecuteAsync(() => throw new CustomException());
        });

        Assert.Equal(3, callbackExecutionCount);
    }

    [Fact(DisplayName = "Test onRetry-Callback invocation with backoff enabled")]
    public async Task Test_OnRetryCallbackExecution_WithExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, true);
        var callbackExecutionCount = 0;

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider
                .GetCappedForeverRetryPolicy<CustomException>(config, (ex, ts, count)
                    => callbackExecutionCount++)
                .ExecuteAsync(() => throw new CustomException());
        });

        Assert.Equal(3, callbackExecutionCount);
    }
    
    */
 
    /*
    [Fact(DisplayName = "Test onRetry-Callback Parameter: 'Timespan' with backoff enabled")]
    public async Task Test_OnRetryCallbackReceivesCorrectDelay_WithExponentialBackoff()
    {
        var config = new RetryConfiguration(3, 100, true);
        var expectedDelays = new[] { 100, 200, 400 };
        var actualDelays = new List<int>();

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider.GetAsyncRetryPolicy<CustomException>(config,
                    (ex, ts, count)
                        => actualDelays.Add((int)ts.TotalMilliseconds))
                .ExecuteAsync(() => throw new CustomException());
        });

        Assert.Equal(expectedDelays, actualDelays);
    }
   
    [Fact(DisplayName = "Test onRetry-Callback Parameter: 'RetryCount' with backoff enabled")]
    public async Task Test_OnRetryCallbackReceivesCorrectRetryCount()
    {
        var config = new RetryConfiguration(3, 100, true);
        var expectedCounts = new[] { 1, 2, 3 };
        var actualCounts = new List<int>();

        await Assert.ThrowsAsync<CustomException>(async () =>
        {
            await RapidMQ.Internals.PolicyProvider
                .GetAsyncRetryPolicy<CustomException>(config, (ex, ts, count)
                    => actualCounts.Add(count))
                .ExecuteAsync(() =>
                {
                    throw new CustomException();
                    return Task.CompletedTask;
                });
        });

        Assert.Equal(expectedCounts, actualCounts);
    }
 */

    internal class CustomException : Exception
    {
    }
}