using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RapidMQ;
using RapidMq_Client;
using RapidMQ.Contracts;
using RapidMQ.Models;

const string connString = "amqp://localhost";
const string queue = "alert.queue", notificationsQueue = "notifications.queue";
const string exchange = "IoT";
const string routingKey = "device.iot.alert", notificationsRoutingKey = "device.notifications.received";

var connectionManager = new ConnectionManager
{
    OnConnectionDrop = eventArgs =>
    {
        Console.WriteLine(eventArgs.Cause);
        return Task.CompletedTask;
    }
};

var logger = new Logger<IRapidMq>(new NullLoggerFactory());
var rapidMqFactory = new RapidMqFactory(connectionManager, logger);

var rapidMq = await rapidMqFactory.CreateAsync(new Uri(connString));

rapidMq.GetOrCreateExchange(exchange, ExchangeType.Topic);

var slowChannel = rapidMq.CreateRapidChannel(new ChannelConfig("MySlowChannel", 5, true));

var alertQueueBinding = rapidMq.CreateQueueBinding(
    new QueueModel(queue, true, false),
    exchange,
    routingKey
);

slowChannel.Listen(alertQueueBinding, new AlertNotifiedEventHandler()); // you can inject these type with DI too


var singleMessageChannel = rapidMq.CreateRapidChannel(new ChannelConfig("SingleMessage", 1, true));

var iotNotificationsBinding = rapidMq.GetOrCreateQueueBinding(
    notificationsQueue,
    exchange,
    notificationsRoutingKey
);

singleMessageChannel.Listen(iotNotificationsBinding, new NotificationEventHandler());


var notificationEvent = new NotificationEvent
{
    Name = "Notification Event",
    NotificationId = 10,
    MemberIds = new[] { 1, 2, 5, 6 }
};

rapidMq.PublishMessage(exchange, notificationsRoutingKey, notificationEvent);

var alertMessage = new AlertNotifiedEvent(100, "2dsadasd", new object[]
{
    new
    {
        Id = 10,
        Casue = "Failure"
    },
    new
    {
        Id = 12,
        Casue = "unknown"
    },
});

rapidMq.PublishMessage(exchange, routingKey, alertMessage);

Console.ReadLine();