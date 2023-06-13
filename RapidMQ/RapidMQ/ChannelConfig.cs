namespace RapidMQ;

public class ChannelConfig
{
    public IList<RapidChannel> Channels { get; set; }

    public ChannelConfig(IList<RapidChannel> channels)
    {
        Channels = channels;
    }

    public ChannelConfig()
    {
        
    }
}