using System.Text.Json;
using RapidMQ.Models;

namespace RapidMq_Client;

public class AlertNotifiedEventHandler : IMqMessageHandler<AlertNotifiedEvent>
{
    public AlertNotifiedEventHandler()
    {
        
    }
    public Task Handle(MessageContext<AlertNotifiedEvent> context)
    {
        var body = context.Message;
        var routingKey = context.RoutingKey;
        var props = context.BasicProperties;
        throw new NotImplementedException();
    }
}