using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMq
{
    public RapidChannel CreateRapidChannel(ChannelConfig channelConfig);
    public Task<RapidChannel> CreateRapidChannelAsync(ChannelConfig channelConfig);
    public QueueBinding CreateQueueBinding(QueueModel queue, string exchangeName, string routingKey);
    public QueueBinding GetOrCreateQueueBinding(string queueName, string exchangeName, string routingKey);
    public void UnbindQueue(string queueName, string exchangeName, string routingKey);
    public void DeclareExchange(string exchangeName, string exchangeType);
    public void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false);
    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage;
    public Task PublishMessageAsync<T>(string exchangeName, string routingKey, T message) where T : IMqMessage;
}