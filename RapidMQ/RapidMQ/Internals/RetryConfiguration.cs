namespace RapidMQ.Internals;

public record RetryConfiguration(int MaxRetries, int MillisecondsBetweenRetries, bool ExponentialBackoffRetry);
