using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RapidMQ.Contracts;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidChannel
{
    private string ChannelName { get; }
    private IModel Channel { get; }
    private EventingBasicConsumer Consumer { get; }

    private readonly Dictionary<string, HandlerAndType> _handlers = new();

    private readonly ILogger<IRapidMq> _logger;

    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    internal RapidChannel(string channelName, IModel channel, EventingBasicConsumer consumer, ILogger<IRapidMq> logger,
        JsonSerializerOptions jsonSerializerOptions = null)
    {
        ChannelName = channelName;
        Channel = channel;
        Consumer = consumer;
        _logger = logger;
        _jsonSerializerOptions = jsonSerializerOptions;
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
                        $"Routing key: {routingKey} has no handler binding associated with it! Channel: {ChannelName}");

                if (handlerObj is null)
                    throw new KeyNotFoundException(
                        $"Routing key: {routingKey} has no handler binding associated with it! Channel: {ChannelName}");

                await ExecuteMessageHandler(args, handlerObj, routingKey);

                Channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Channel: {Name} - An error occurred while processing the message.", ChannelName);
                try
                {
                    Channel.BasicNack(args.DeliveryTag, false, false);
                }
                catch (AlreadyClosedException ex)
                {
                    _logger.LogError(ex, "Channel: {Name} -  An error occurred while sending nack to the broker.",
                        ChannelName);
                }
            }
        };
    }

    private async Task ExecuteMessageHandler(BasicDeliverEventArgs args, HandlerAndType handlerObj, string routingKey)
    {
        var body = Encoding.UTF8.GetString(args.Body.ToArray());
        var message = JsonSerializer.Deserialize(body, handlerObj.MessageType,
            _jsonSerializerOptions ?? SerializingConfig.DefaultOptions);

        if (message == null)
            throw new ArgumentNullException(nameof(args),
                $"Channel: {ChannelName} - Message is null after deserializing the body! RoutingKey:{routingKey}, Body: {body}");

        var messageContext = new MessageContext<IMqMessage>
        {
            Message = (IMqMessage)message,
            DeliveryTag = args.DeliveryTag,
            RoutingKey = routingKey,
            BasicProperties = args.BasicProperties
        };

        await handlerObj.Handler(messageContext);
    }
}