using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public class RapidMq
{
    private readonly string _connectionString;
    private readonly IConnection _connection;
    private readonly ChannelManager _channelManager;
    private readonly HashSet<QueueBinding> _queueBindings = new();

    private readonly IModel _channel;

    public RapidMq(string connectionString, List<ChannelConfig> channelConfig)
    {
        _connectionString = connectionString;
        _connection = Connect();
        _channelManager = new ChannelManager(_connection, channelConfig);
        _channel = CreateChannel();
    }

    public QueueBinding CreateBinding(QueueModel queue, string exchangeName, string routingKey)
    {
        DeclareQueue(queue.QueueName, queue.Durable, queue.AutoDelete);
        _channel.QueueBind(queue.QueueName, exchangeName, routingKey);
        var binding = new QueueBinding(queue.QueueName, routingKey, exchangeName);
        _queueBindings.Add(binding);
        return binding;
    }

    public void Listen(string channelName, QueueBinding queueBinding, Action action)
    {
        var exists = _queueBindings.TryGetValue(queueBinding, out var createBinding);
        if (!exists)
        {
            createBinding = CreateBinding(QueueModel.DefaultQueue(queueBinding.QueueName), queueBinding.ExchangeName,
                queueBinding.RoutingKey);
            _queueBindings.Add(createBinding);
        }

        var rapidChannel = _channelManager.RegisterEventHandler(channelName, queueBinding.RoutingKey, action);
        rapidChannel.ConsumeFromQueue(queueBinding.QueueName);
    }

    private IConnection Connect()
    {
        return new ConnectionFactory
        {
            Uri = new Uri(_connectionString)
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
        _channel.ExchangeDeclare(exchangeName, exchangeType, true);
    }

    private void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false)
    {
        _channel.QueueDeclare(queueName, durable, false, autoDelete);
    }


    private void BindQueue(string queueName, string routingKey, string exchange)
    {
        _channel.QueueBind(queueName, exchange, routingKey);
        //_channel.BasicConsume(queueName, false, consumer: _consumer);
    }
}