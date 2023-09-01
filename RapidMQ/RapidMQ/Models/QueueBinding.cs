namespace RapidMQ.Models;

/// <summary>
/// Represents a queue binding between a queue and an exchange through a routing key
/// </summary>
/// <param name="QueueName"></param>
/// <param name="RoutingKey"></param>
/// <param name="ExchangeName"></param>
public record QueueBinding(string QueueName, string RoutingKey, string ExchangeName);