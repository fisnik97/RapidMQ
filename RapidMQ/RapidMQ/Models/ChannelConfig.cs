namespace RapidMQ.Models;

public record ChannelConfig(string Id, ushort PrefetchCount, bool IsGlobal = true);