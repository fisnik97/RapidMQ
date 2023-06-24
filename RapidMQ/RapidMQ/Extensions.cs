using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public static class Extensions
{
    public static string PayloadToString(this BasicDeliverEventArgs args)
    {
        var document = JsonDocument.Parse(Encoding.UTF8.GetString(args.Body.Span));
        return $"Exchange: {args.Exchange}, RoutingKey: {args.RoutingKey}, Body: {document.RootElement.ToString()}";
    }
}