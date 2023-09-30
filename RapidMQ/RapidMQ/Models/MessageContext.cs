using RabbitMQ.Client;
using RapidMQ.Contracts;

namespace RapidMQ.Models;

/// <summary>
/// RapidMq message context enriched with RabbitMQ properties 
/// </summary>
/// <typeparam name="T"></typeparam>
public class MessageContext<T> where T : IMqMessage
{
    /// <summary>
    /// Body or payload of the message
    /// </summary>
    public T Message { get; init; }

    /// <summary>
    /// RabbitMQ BasicProperties of the message
    /// </summary>
    public IBasicProperties BasicProperties { get; init; }

    /// <summary>
    /// Routing key of the message
    /// </summary>
    public string RoutingKey { get; init; }

    /// <summary>
    /// Delivery tag of the message
    /// </summary>
    public ulong DeliveryTag { get; init; }
}