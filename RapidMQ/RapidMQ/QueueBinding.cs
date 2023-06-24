namespace RapidMQ;

public record QueueBinding(string QueueName, string RoutingKey, string ExchangeName);