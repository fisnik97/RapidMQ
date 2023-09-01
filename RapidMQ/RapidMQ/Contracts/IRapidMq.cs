using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMq
{
    /// <summary>
    /// Creates a new RapidChannel instance with the given configuration.
    /// </summary>
    /// <param name="channelConfig"></param>
    /// <returns></returns>
    public RapidChannel CreateRapidChannel(ChannelConfig channelConfig);
    
    /// <summary>
    /// Gets or creates a new QueueBinding instance with the given parameters.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <returns></returns>
    public QueueBinding GetOrCreateQueueBinding(QueueModel queue, string exchangeName, string routingKey);
    /// <summary>
    /// Gets or creates a new QueueBinding instance with the given parameters, using the default queue settings
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <returns></returns>
    public QueueBinding GetOrCreateQueueBinding(string queueName, string exchangeName, string routingKey);
    
    /// <summary>
    /// Unbinds the given queue from the given exchange with the given routing key.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    public void UnbindQueue(string queueName, string exchangeName, string routingKey);
    
    /// <summary>
    /// Gets or creates a new Exchange instance with the given parameters.
    /// </summary>
    /// <param name="exchangeName"></param>
    /// <param name="exchangeType"></param>
    /// <returns></returns>
    public string GetOrCreateExchange(string exchangeName, string exchangeType);
    
    /// <summary>
    /// Declares a new queue with the given parameters.
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="durable"></param>
    /// <param name="autoDelete"></param>
    /// <returns></returns>
    public string DeclareQueue(string queueName, bool durable = true, bool autoDelete = false);
    
    /// <summary>
    /// Publishes the given message to the given exchange with the given routing key.
    /// </summary>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <param name="message"></param>
    /// <typeparam name="T"></typeparam>
    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage;
}