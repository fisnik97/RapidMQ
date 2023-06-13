using RabbitMQ.Client;

namespace RapidMQ;

public class RapidChannel
{
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RapidChannel(IConnection connection, string channelName, ushort prefetchCount, bool isGlobal)
    {
        _connection = connection;
        ChannelName = channelName;
        PrefetchCount = prefetchCount;
        IsGlobal = isGlobal;
        _channel = CreateMChannel();
    }

    private ushort PrefetchCount { get; init; }
    private bool IsGlobal { get; init; }
    public string? ChannelName { get; init; }

    private IModel CreateMChannel()
    {
        var model = _connection.CreateModel();
        model.BasicQos(0, PrefetchCount, IsGlobal);
        return model;
    }

    public static IModel ToIModel(RapidChannel self) => self._channel;
}