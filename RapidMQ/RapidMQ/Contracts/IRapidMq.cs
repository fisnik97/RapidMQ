using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMq
{
    public RapidChannel CreateRapidChannel(ChannelConfig channelConfig);
    public QueueBinding GetOrCreateQueueBinding(QueueModel queue, string exchangeName, string routingKey);
    public QueueBinding GetOrCreateQueueBinding(string queueName, string exchangeName, string routingKey);
    public void UnbindQueue(string queueName, string exchangeName, string routingKey);
    public string GetOrCreateExchange(string exchangeName, string exchangeType);
    public string DeclareQueue(string queueName, bool durable = true, bool autoDelete = false);
    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage;
}