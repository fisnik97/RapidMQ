using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidChannel
{
    private string ChannelName { get; }
    private IModel Channel { get; }
    private EventingBasicConsumer Consumer { get; }
    private readonly Dictionary<string, Action> _eventBindings = new();

    public RapidChannel(string channelName, IModel channel, EventingBasicConsumer consumer)
    {
        ChannelName = channelName;
        Channel = channel;
        Consumer = consumer;
        DefineConsumerHandler();
    }

    private void EnsureUniqueEventHandler(string routingKey, Action handler)
    {
        if (_eventBindings.ContainsKey(routingKey))
            throw new InvalidOperationException(
                $"There is a handler already defined for routing key: ${routingKey} in channel: {ChannelName}");
        
        _eventBindings.Add(routingKey, handler);
    }

    public void Listen(QueueBinding queueBinding, Action action)
    {
        EnsureUniqueEventHandler(queueBinding.RoutingKey, action);

        Channel.BasicConsume(queueBinding.QueueName, false, Consumer);
    }

    private void DefineConsumerHandler()
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