namespace RapidMQ.Models;

/// <summary>
/// Represents a queue binding between a queue and an exchange through a routing key
/// </summary>
public record QueueBinding
{
    public QueueBinding(string QueueName, string RoutingKey, string ExchangeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(QueueName, "QueueName cannot be empty!");
        ArgumentException.ThrowIfNullOrEmpty(RoutingKey, "RoutingKey cannot be empty!");
        ArgumentException.ThrowIfNullOrEmpty(ExchangeName, "ExchangeName cannot be empty!");

        this.QueueName = QueueName;
        this.RoutingKey = RoutingKey;
        this.ExchangeName = ExchangeName;
    }

    /// <summary>
    /// QueueName used in the binding
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Routing key used to bind the queue to the exchange
    /// </summary>
    public string RoutingKey { get; }

    /// <summary>
    /// Exchange name used in the binding
    /// </summary>
    public string ExchangeName { get; }
}