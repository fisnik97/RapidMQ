using RapidMQ.Models;
using WebClient.Events;
using WebClient.Services;

namespace WebClient.EventHandlers;

public class AlertReceivedEventHandler : IMqMessageHandler<AlertReceivedEvent>
{
    private readonly ISomeService _someService;

    public AlertReceivedEventHandler(ISomeService someService)
    {
        _someService = someService;
    }

    public async Task Handle(MessageContext<AlertReceivedEvent> context)
    {
        Console.WriteLine($"Processing event with payload: {context.Message}");

        await _someService.DoSomethingAsync();

        Console.WriteLine(
            $"Processing event with payload: {context.Message} and routingKey: {context.RoutingKey} completed");
    }
}