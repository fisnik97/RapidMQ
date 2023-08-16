using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidChannel
{
    private string ChannelName { get; }
    private IModel Channel { get; }
    private EventingBasicConsumer Consumer { get; }

    private readonly Dictionary<string, HandlerAndType> _handlers = new();

    internal RapidChannel(string channelName, IModel channel, EventingBasicConsumer consumer)
    {
        ChannelName = channelName;
        Channel = channel;
        Consumer = consumer;
        DefineConsumerHandler();
    }

    public void Listen<T>(QueueBinding queueBinding, IMqMessageHandler<T> handler) where T : IMqMessage
    {
        var handlerAndType = new HandlerAndType
        {
            Handler = (msgContext) => handler.Handle(
                new MessageContext<T>
                {
                    Message = (T)msgContext.Message,
                    DeliveryTag = msgContext.DeliveryTag,
                    RoutingKey = msgContext.RoutingKey,
                    BasicProperties = msgContext.BasicProperties
                }),
            MessageType = typeof(T)
        };

        _handlers[queueBinding.RoutingKey] = handlerAndType;
        Channel.BasicConsume(queueBinding.QueueName, false, Consumer);
    }

    private void DefineConsumerHandler()
    {
        Consumer.Received += async (_, args) =>
        {
            var routingKey = args.RoutingKey;
            try
            {
                if (!_handlers.TryGetValue(routingKey, out var handlerObj))
                    throw new KeyNotFoundException(
                        $"Routing key: {routingKey} has no handler binding associated with it!");

                if (handlerObj is null)
                    throw new KeyNotFoundException(
                        $"Routing key: {routingKey} has no handler binding associated with it!");

                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize(body, handlerObj.MessageType,
                    SerializingConfig.DefaultOptions);

                if (message != null)
                    await handlerObj.Handler(new MessageContext<IMqMessage>
                    {
                        Message = (IMqMessage)message,
                        DeliveryTag = args.DeliveryTag,
                        RoutingKey = routingKey,
                        BasicProperties = args.BasicProperties
                    });

                Channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception e)
            {
                Channel.BasicNack(args.DeliveryTag, false, false);
            }
        };
    }
}