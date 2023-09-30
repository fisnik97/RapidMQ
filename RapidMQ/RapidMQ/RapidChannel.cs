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
    private readonly string _channelName;
    private readonly ChannelConfig _channelConfig;
    private IModel Channel { get; set; }
    private EventingBasicConsumer Consumer { get; set; }

    private readonly Dictionary<string, HandlerAndType> _handlers = new();

    private readonly ILogger<IRapidMq> _logger;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Creates a new RapidChannel instance with the given configuration.
    /// </summary>
    /// <param name="channelName">Unique channel name</param>
    /// <param name="channelConfig">Configuration for the rapid-channel</param>
    /// <param name="connection">Connection instance</param>
    /// <param name="logger">Logger of RapidMq instance</param>
    /// <param name="jsonSerializerOptions">Custom serializer of channel</param>
    /// <exception cref="ArgumentNullException"></exception>
    internal RapidChannel(string channelName, ChannelConfig channelConfig, IConnection connection,
        ILogger<IRapidMq> logger,
        JsonSerializerOptions jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        _channelName = channelName ?? throw new ArgumentNullException(nameof(channelName));
        _channelConfig = channelConfig ?? throw new ArgumentNullException(nameof(channelConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptions = jsonSerializerOptions;

        InitChannel(connection);
        InitChannelConsumer();
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
                        $"Routing key: {routingKey} has no handler binding associated with it! Channel: {_channelName}");

                if (handlerObj is null)
                    throw new KeyNotFoundException(
                        $"Routing key: {routingKey} has no handler binding associated with it! Channel: {_channelName}");

                await ExecuteMessageHandler(args, handlerObj, routingKey);

                Channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Channel: {Name} - An error occurred while processing the message.", _channelName);
                try
                {
                    Channel.BasicNack(args.DeliveryTag, false, false);
                }
                catch (AlreadyClosedException ex)
                {
                    _logger.LogError(ex, "Channel: {Name} -  An error occurred while sending nack to the broker.",
                        _channelName);
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
                $"Channel: {_channelName} - Message is null after deserializing the body! RoutingKey:{routingKey}, Body: {body}");

        var messageContext = new MessageContext<IMqMessage>
        {
            Message = (IMqMessage)message,
            DeliveryTag = args.DeliveryTag,
            RoutingKey = routingKey,
            BasicProperties = args.BasicProperties
        };

        await handlerObj.Handler(messageContext);
    }

    private void InitChannel(IConnection connection)
    {
        try
        {
            Channel = connection.CreateModel();
            Channel.BasicQos(0, _channelConfig.PrefetchCount, _channelConfig.IsGlobal);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Channel: {Name} - An error occurred while creating the channel.", _channelName);
            throw;
        }
    }

    private void InitChannelConsumer()
    {
        if (Channel == null)
        {
            throw new ArgumentNullException(nameof(Channel),
                $"Channel: {_channelName} - Channel is null! Cannot create consumer.");
        }

        Consumer = new EventingBasicConsumer(Channel);
    }
}