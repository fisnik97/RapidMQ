using RapidMQ.Models;

namespace RapidMq_Client;

public class NotificationEventHandler : IMqMessageHandler<NotificationEvent>
{
    public async Task Handle(MessageContext<NotificationEvent> context)
    {
        var name = context.Message.Name;
        Console.WriteLine(context.Message.MemberIds);
        await Task.Delay(100);
    }
}