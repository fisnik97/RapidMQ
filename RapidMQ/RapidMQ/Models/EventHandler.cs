using RapidMQ.Contracts;

namespace RapidMQ.Models;

internal record HandlerAndType
{
    /// <summary>
    /// Message handler to be called when a message is received
    /// </summary>
    public Func<MessageContext<IMqMessage>, Task> Handler { get; init; }

    /// <summary>
    /// Type of message to be handled. Needed when deserializing the message
    /// </summary>
    public Type MessageType { get; init; }
}