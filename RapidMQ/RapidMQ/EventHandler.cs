using RapidMQ.Models;

namespace RapidMQ;

internal record HandlerAndType
{
    public Func<MessageContext<IMqMessage>, Task> Handler { get; init; }
    public Type MessageType { get; init; }
}