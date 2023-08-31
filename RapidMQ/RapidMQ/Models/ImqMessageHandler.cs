namespace RapidMQ.Models;

/// <summary>
/// Base message handler interface for all messages received in RapidMQ
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMqMessageHandler<T> where T : IMqMessage
{
    public Task Handle(MessageContext<T> context);
}