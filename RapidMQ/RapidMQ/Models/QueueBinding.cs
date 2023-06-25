namespace RapidMQ.Models;

public record QueueBinding(string QueueName, string RoutingKey, string ExchangeName);