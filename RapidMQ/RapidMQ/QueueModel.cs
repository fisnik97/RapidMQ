namespace RapidMQ;

public record QueueModel(string QueueName, bool Durable, bool AutoDelete)
{
    public static readonly Func<string, QueueModel> DefaultQueue = (string queueName) =>
        new QueueModel(queueName, true, false);
}