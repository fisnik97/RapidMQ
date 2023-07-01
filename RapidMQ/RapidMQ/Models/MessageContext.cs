using RabbitMQ.Client;

namespace RapidMQ.Models;

public class MessageContext<T> where T : IMqMessage
{
    public T Message { get; set; }
    public IBasicProperties BasicProperties { get; set; }
    public string RoutingKey { get; set; }
    public ulong DeliveryTag { get; set; }
}