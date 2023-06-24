namespace RapidMQ;

public record ChannelConfig(string ChannelName, ushort PrefetchCount, bool IsGlobal = true);