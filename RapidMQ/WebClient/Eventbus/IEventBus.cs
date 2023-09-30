using RapidMQ.Contracts;
using RapidMQ.Models;

namespace WebClient.Eventbus;

public interface IEventBus
{
    void PublishEvent<TEvent>(string exchangeName, TEvent @event) where TEvent : IMqMessage;
}