namespace RapidMQ.Models;

/// <summary>
/// Channel configuration used to create a new RapidChannel
/// </summary>
public record ChannelConfig
{
    public ChannelConfig(string ChannelName, ushort PrefetchCount, bool IsGlobal = true)
    {
        if (PrefetchCount < 1)
            throw new ArgumentOutOfRangeException(nameof(PrefetchCount), "PrefetchCount must be greater than 0");

        this.ChannelName = ChannelName;
        this.PrefetchCount = PrefetchCount;
        this.IsGlobal = IsGlobal;
    }

    /// <summary>
    /// A given name for a channel to distinguish it from others 
    /// </summary>
    public string ChannelName { get; init; }

    /// <summary>
    /// Number of unacknowledged messages that are allowed to be processed at a time  
    /// </summary>
    public ushort PrefetchCount { get; init; }

    /// <summary>
    /// Indicates whether the prefetchSize and prefetchCount settings should apply to the entire channel or per consumer.
    /// </summary>
    public bool IsGlobal { get; init; }
}