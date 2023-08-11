using RapidMQ.Models;

namespace WebClient;

public interface IEventBus
{
    void PublishEvent<TEvent>(string routingKey, string exchangeName, TEvent @event) where TEvent : IMqMessage;
}