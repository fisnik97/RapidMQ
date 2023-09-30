using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Events;
using WebClient.Services;

namespace WebClient.EventHandlers;

public class NotificationEventHandler : IMqMessageHandler<NotificationEvent>
{

    private readonly ISomeService _someService;

    public NotificationEventHandler(ISomeService someService)
    {
        _someService = someService;
    }

    public async Task Handle(MessageContext<NotificationEvent> context)
    {
        var notification = context.Message;
        Console.WriteLine($"Notification received: {notification.NotificationId}");
        await _someService.DoSomethingAsync();
    }
}