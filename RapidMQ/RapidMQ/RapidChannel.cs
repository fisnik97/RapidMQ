using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidChannel
{
    private string ChannelName { get; }
    private IModel Channel { get; }
    private EventingBasicConsumer Consumer { get; }

    private readonly Dictionary<string, Func<MessageContext<IMqMessage>, Task>> _handlers = new();

    internal RapidChannel(string channelName, IModel channel, EventingBasicConsumer consumer)
    {
        ChannelName = channelName;
        Channel = channel;
        Consumer = consumer;
        DefineConsumerHandler();
    }

    public void Listen<T>(QueueBinding queueBinding, IMqMessageHandler<T> handler) where T : IMqMessage
    {
        _handlers[queueBinding.RoutingKey] = context =>
        {
         

            return handler.Handle(null);
        };
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
                {
                    Channel.BasicAck(args.DeliveryTag, true);
                    return;
                };

                if (handlerObj is null)
                    throw new InvalidCastException("Handler cannot be cast to correct type");

                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonSerializer.Deserialize<IMqMessage>(body);

                if (message != null)
                    await handlerObj(new MessageContext<IMqMessage>
                    {
                        Message = message,
                        DeliveryTag = args.DeliveryTag
                    });

                Channel.BasicAck(args.DeliveryTag, true);
            }
            catch (Exception e)
            {
                Channel.BasicNack(args.DeliveryTag, true, false);
            }
        };
    }
}