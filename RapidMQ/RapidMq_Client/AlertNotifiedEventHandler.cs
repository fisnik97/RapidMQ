using System.Text.Json;
using RapidMQ.Models;

namespace RapidMq_Client;

public class AlertNotifiedEventHandler : IMqMessageHandler<AlertNotifiedEvent>
{
    public AlertNotifiedEventHandler()
    {
        
    }
    public async Task Handle(MessageContext<AlertNotifiedEvent> context)
    {
        var body = context.Message;
        var device = body.DeviceId;
        if (device == default)
        {
            throw new ArgumentNullException(nameof(body), "Empty device not allowed");
        }
        var routingKey = context.RoutingKey;
        var props = context.BasicProperties;

        await Task.Delay(100);
    }
}