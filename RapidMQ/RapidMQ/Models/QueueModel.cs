namespace RapidMQ.Models;

/// <summary>
/// Basic queue model for creating a queue
/// </summary>
/// <param name="QueueName"></param>
/// <param name="Durable"></param>
/// <param name="AutoDelete"></param>
public record QueueModel(string QueueName, bool Durable, bool AutoDelete)
{
    public static readonly Func<string, QueueModel> DefaultQueue = (queueName) =>
        new QueueModel(queueName, true, false);
}