using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public class RapidChannel
{
    public string ChannelName { get; set; }
    public IModel Channel { get; set; }
    public EventingBasicConsumer Consumer { get; set; }
    private readonly Dictionary<string, Action> _eventBindings = new();

    public RapidChannel(string channelName, IModel channel, EventingBasicConsumer consumer)
    {
        ChannelName = channelName;
        Channel = channel;
        Consumer = consumer;
        StartListening();
    }

    public void AddEventBinding(string routingKey, Action handler)
    {
        _eventBindings.Add(routingKey, handler);
    }

    public void ConsumeFromQueue(string queueName)
    {
        Channel.BasicConsume(queueName, false, consumer: Consumer);
    }

    private void StartListening()
    {
        Consumer.Received += (sender, args) =>
        {
            var routingKey = args.RoutingKey;
            try
            {
                var handler = _eventBindings.TryGetValue(routingKey, out var actionHandler);

                actionHandler?.Invoke();

                Channel.BasicAck(args.DeliveryTag, true);
            }
            catch (Exception e)
            {
                Channel.BasicNack(args.DeliveryTag, true, false);
            }
        };
    }
}