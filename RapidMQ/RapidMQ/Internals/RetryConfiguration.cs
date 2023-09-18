namespace RapidMQ.Internals;

public record RetryConfiguration(long MaxMillisecondsDelay, int InitialMillisecondsRetry);
