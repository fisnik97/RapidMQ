using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RapidMQ;

public class RapidMq
{
    private readonly string _connectionString;
    private readonly IModel _channel;
    private readonly EventingBasicConsumer _consumer;
    private readonly IConnection _connection;
    private readonly ChannelConfig _channelConfig;

    public RapidMq(string connectionString, ChannelConfig channelConfig)
    {
        _connectionString = connectionString;
        _channelConfig = channelConfig;
        _connection = Connect();
        _channel = CreateChannel();
        _consumer = new EventingBasicConsumer(_channel);
    }

    private IConnection Connect()
    {
        var connManager = new ConnectionFactory
        {
            Uri = new Uri(_connectionString)
        };
        return connManager.CreateConnection();
    }

    private IModel CreateChannel()
    {
        var channel = _connection.CreateModel();
        channel.BasicQos(0, 1, false);
        return channel;
    }

    public void PublishEvent(string exchange, string routingKey, object payload)
    {
        _channel.BasicPublish(exchange, routingKey, true, null);
    }

    public void StartListening()
    {
        _consumer.Received += async (sender, args) =>
        {
            await Task.Delay(10);
            Console.WriteLine("An event came");
            var payload = args.PayloadToString();
            Console.WriteLine(payload);
            _channel.BasicAck(args.DeliveryTag, false);
        };
    }

    public void DeclareExchange(string exchangeName, string exchangeType)
    {
        _channel.ExchangeDeclare(exchangeName, exchangeType, true);
    }

    public void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false)
    {
        _channel.QueueDeclare(queueName, durable, false, autoDelete);
    }

    public void BindQueue(string queueName, string routingKey, string exchange)
    {
        _channel.QueueBind(queueName, exchange, routingKey);
        _channel.BasicConsume(queueName, false, consumer: _consumer);
    }
}