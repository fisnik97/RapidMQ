namespace RapidMQ.Models;

public interface IMqMessageHandler<T> where T : IMqMessage
{
    public Task Handle(MessageContext<T> context);
}