using System.Reflection;
using RapidMQ.Contracts;
using RapidMQ.Models;

namespace WebClient;

public class EventBus : IEventBus
{
    private readonly IRapidMq _rapidMq;

    public EventBus(IRapidMq rapidMq)
    {
        _rapidMq = rapidMq;
    }

    public void PublishEvent<TEvent>(string routingKey, string exchangeName, TEvent @event) where TEvent : IMqMessage
    {
        var eventRoutingKey = GetMqEventAttributeValue<TEvent>();
        _rapidMq.PublishMessage(exchangeName, eventRoutingKey, @event);
    }

    private static string GetMqEventAttributeValue<T>() where T : IMqMessage
    {
        var attribute = typeof(T).GetCustomAttribute<MqEventAttribute>();

        if (attribute?.RoutingKey == null)
            throw new ArgumentNullException(nameof(attribute));

        return attribute.RoutingKey;
    }
}