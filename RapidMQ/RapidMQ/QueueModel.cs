namespace RapidMQ;

public record QueueModel(string QueueName, bool Durable, bool AutoDelete)
{
    public static Func<string, QueueModel> DefaultQueue = (string queueName) =>
        new QueueModel(queueName, true, false);
}