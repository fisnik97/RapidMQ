namespace RapidMQ.Models;

/// <summary>
/// Channel configuration used to create a new RapidChannel
/// </summary>
/// <param name="Id"></param>
/// <param name="PrefetchCount"></param>
/// <param name="IsGlobal"></param>
public record ChannelConfig(string Id, ushort PrefetchCount, bool IsGlobal = true);