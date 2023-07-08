using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMq
{
    public RapidChannel CreateChannel(ChannelConfig channelConfig);
    public QueueBinding CreateQueueBinding(QueueModel queue, string exchangeName, string routingKey);
    public QueueBinding CreateQueueBinding(string queueName, string exchangeName, string routingKey);
    public void UnbindQueue(string queueName, string exchangeName, string routingKey);
    public void DeclareExchange(string exchangeName, string exchangeType);
    public void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false);
    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage;
}