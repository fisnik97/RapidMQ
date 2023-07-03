using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidMq
{
    private readonly Uri _connectionString;
    private IConnection _connection = null!;
    private readonly HashSet<QueueBinding> _queueBindings = new();
    private IModel _setupChannel = null!;

    public static async Task<RapidMq> CreateAsync(Uri uri)
    {
        var rapidMq = new RapidMq(uri);
        await rapidMq.Connect();
        await rapidMq.CreateChannel();
        return rapidMq;
    }

    private RapidMq(Uri connectionString)
    {
        _connectionString = connectionString;
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

    private async Task Connect()
    {
        var policy = PolicyProvider.GetBackOffRetryPolicy(5, 2,
            (exception, span, attempt) =>
            {
                Console.WriteLine(
                    $"Could not connect to the RabbitMQ server after {attempt}(s) attempt, timespan: {span}! - {exception.Message}");
            });

        await policy.ExecuteAsync(async (cancellationToken) =>
        {
            try
            {
                var conn = new ConnectionFactory
                {
                    Uri = _connectionString
                }.CreateConnection();
                _connection = conn;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect: {e.Message}");
                throw;
            }
        }, CancellationToken.None);
    }

    private async Task CreateChannel()
    {
        var policy = PolicyProvider.GetLinearRetryPolicy(5, 2,
            (exception, span, retryAttempt) =>
            {
                Console.WriteLine(
                    $"Could not connect to the SetupChannel after ${retryAttempt}(s) attempt, timespan: {span}! - {exception.Message}");
            });

        await policy.ExecuteAsync(async (cancellationToken) =>
        {
            try
            {
                var channel = _connection.CreateModel();
                channel.BasicQos(0, 1, false);
                _setupChannel = channel;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to setup the channel: {e.Message}");
                throw;
            }
        }, CancellationToken.None);
    }

    public void DeclareExchange(string exchangeName, string exchangeType)
    {
        _setupChannel.ExchangeDeclare(exchangeName, exchangeType, true);
    }

    private void DeclareQueue(string queueName, bool durable = true, bool autoDelete = false)
    {
        _setupChannel.QueueDeclare(queueName, durable, false, autoDelete);
    }

    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage
    {
        using var channel = _connection.CreateModel();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchangeName, routingKey, body: body);
    }
}