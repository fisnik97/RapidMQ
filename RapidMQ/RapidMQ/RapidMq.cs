using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidMq
{
    private readonly Uri _connectionString;
    private readonly IConnection _connection;
    private readonly HashSet<QueueBinding> _queueBindings = new();
    private readonly IModel _setupChannel;

    public RapidMq(Uri connectionString)
    {
        _connectionString = connectionString;
        _connection = Connect();
        _setupChannel = CreateChannel();
    }

    public RapidChannel CreateChannel(ChannelConfig channelConfig)
    {
        var channel = _connection.CreateModel();
        channel.BasicQos(0, channelConfig.PrefetchCount, channelConfig.IsGlobal);

        var consumer = new EventingBasicConsumer(channel);
        var rapidChannel = new RapidChannel(channelConfig.Id, channel, consumer);
        return rapidChannel;
    }

    public QueueBinding CreateQueueBinding(QueueModel queue, string exchangeName, string routingKey)
    {
        var existingBinding = _queueBindings.FirstOrDefault(x =>
            x.QueueName == queue.QueueName && x.RoutingKey == routingKey && x.ExchangeName == exchangeName);

        if (existingBinding != null)
            return existingBinding;

        DeclareQueue(queue.QueueName, queue.Durable, queue.AutoDelete);
        _setupChannel.QueueBind(queue.QueueName, exchangeName, routingKey);

        var binding = new QueueBinding(queue.QueueName, routingKey, exchangeName);
        _queueBindings.Add(binding);

        return binding;
    }

    public QueueBinding CreateQueueBinding(string queue, string exchangeName, string routingKey)
    {
        var existingBinding = _queueBindings.FirstOrDefault(x =>
            x.QueueName == queue && x.RoutingKey == routingKey && x.ExchangeName == exchangeName);

        if (existingBinding != null)
            return existingBinding;

        var defaultQueue = QueueModel.DefaultQueue(queue);
        DeclareQueue(defaultQueue.QueueName, defaultQueue.Durable, defaultQueue.AutoDelete);
        _setupChannel.QueueBind(queue, exchangeName, routingKey);

        var binding = new QueueBinding(queue, routingKey, exchangeName);
        _queueBindings.Add(binding);

        return binding;
    }

    private IConnection Connect()
    {
        return new ConnectionFactory
        {
            Uri = _connectionString
        }.CreateConnection();
    }

    private IModel CreateChannel()
    {
        var channel = _connection.CreateModel();
        channel.BasicQos(0, 1, false);
        return channel;
    }

    public void DeclareExchange(string exchangeName, string exchangeType)
    {
        _setupChannel.ExchangeDeclare(exchangeName, exchangeType, true);
    }

    private void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false)
    {
        _setupChannel.QueueDeclare(queueName, durable, false, autoDelete);
    }
}