using RapidMQ.Contracts;

namespace WebClient.Eventbus;

public interface IEventBus
{
    void PublishEvent<TEvent>(string exchangeName, TEvent @event) where TEvent : IMqMessage;
}