using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public static class Extensions
{
    public static string PayloadToString(this BasicDeliverEventArgs args)
    {
        var body = Encoding.UTF8.GetString(args.Body.Span);
        var document = JsonDocument.Parse(body);
        var bodyPayload = document.RootElement.ToString();
        return $"Exchange: {args.Exchange}, RoutingKey: {args.RoutingKey}, Body: {bodyPayload}";
    }
}