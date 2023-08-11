﻿using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RapidMQ.Contracts;
using RapidMQ.Internals;
using RapidMQ.Models;

namespace RapidMQ;

public class RapidMq : IRapidMq
{
    private readonly IConnection _connection;
    private readonly HashSet<QueueBinding> _queueBindings = new();
    private readonly IModel _setupChannel;

    private readonly ILogger<IRapidMq> _logger;

    public RapidMq(IConnection connection, ILogger<IRapidMq> logger)
    {
        _connection = connection;
        _logger = logger;
        _setupChannel = _connection.CreateModel();

        _connection.ConnectionShutdown += (sender, args) =>
        {
            if (args.Initiator == ShutdownInitiator.Application)
                return;
            //Implement the logic to re-create the rapid channels 
        };
    }

    public RapidChannel CreateRapidChannel(ChannelConfig channelConfig)
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

    public QueueBinding GetOrCreateQueueBinding(string queue, string exchangeName, string routingKey)
    {
        var existingBinding = _queueBindings.SingleOrDefault(x =>
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

    public void UnbindQueue(string queueName, string exchangeName, string routingKey)
    {
        var binding = new QueueBinding(queueName, exchangeName, routingKey);

        if (_queueBindings.TryGetValue(binding, out var existingBinding))
            _queueBindings.Remove(existingBinding);

        _setupChannel.QueueUnbind(binding.QueueName, binding.ExchangeName, binding.RoutingKey);
    }

    public string GetOrCreateExchange(string exchangeName, string exchangeType)
    {
        _setupChannel.ExchangeDeclare(exchangeName, exchangeType, true);
        return exchangeName;
    }

    public string DeclareQueue(string queueName, bool durable = true, bool autoDelete = false)
    {
        var data = _setupChannel.QueueDeclare(queueName, durable, false, autoDelete);
        return data.QueueName;
    }

    public void PublishMessage<T>(string exchangeName, string routingKey, T message) where T : IMqMessage
    {
        using var channel = _connection.CreateModel();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, SerializingConfig.DefaultOptions));
        channel.BasicPublish(exchangeName, routingKey, body: body);
    }
}