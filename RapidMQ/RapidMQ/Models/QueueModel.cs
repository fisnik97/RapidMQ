namespace RapidMQ.Models;

public record QueueModel(string QueueName, bool Durable, bool AutoDelete)
{
    public static readonly Func<string, QueueModel> DefaultQueue = (queueName) =>
        new QueueModel(queueName, true, false);
}