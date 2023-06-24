using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public class ChannelManager
{
    private readonly List<ChannelConfig> _channelConfigs;
    private readonly IConnection _connection;
    private readonly List<RapidChannel> _rapidChannels = new();


    public ChannelManager(IConnection connection, List<ChannelConfig> channelsConfigs)
    {
        if (channelsConfigs.Count == 0)
            throw new ArgumentNullException(nameof(channelsConfigs));

        _connection = connection;
        _channelConfigs = channelsConfigs;
        BuildChannels();
    }

    public RapidChannel RegisterEventHandler(string channelName, string routingKey, Action handler)
    {
        var rapidChannel = _rapidChannels.FirstOrDefault(x => x.ChannelName.Equals(channelName));

        if (rapidChannel == null)
            throw new ArgumentNullException(nameof(channelName),
                $"RapidChannel with name: {channelName} was not found!");

        rapidChannel.AddEventBinding(routingKey, handler);

        return rapidChannel;
    }

    private IModel CreateChannel(ChannelConfig channelConfig)
    {
        var channel = _connection.CreateModel();
        channel.BasicQos(0, channelConfig.PrefetchCount, channelConfig.IsGlobal);
        return channel;
    }

    private EventingBasicConsumer CreateConsumer(IModel channel)
    {
        return new EventingBasicConsumer(channel);
    }

    private void BuildChannels()
    {
        foreach (var channelConfig in _channelConfigs)
        {
            var channel = CreateChannel(channelConfig);
            var consumer = CreateConsumer(channel);
            var rapidChannel = new RapidChannel(channelConfig.ChannelName, channel, consumer);
            _rapidChannels.Add(rapidChannel);
        }
    }
}