using RabbitMQ.Client;
using RapidMQ;
using RapidMq_Client;
using RapidMQ.Models;

const string connString = "amqp://localhost";

var rapidMq = await RapidMq.CreateAsync(new Uri(connString));


const string queue = "alert.queue", notificationsQueue = "notifications.queue";
const string exchange = "IoT";
const string routingKey = "device.iot.alert", notificationsRoutingKey = "device.notifications.received";

rapidMq.DeclareExchange(exchange, ExchangeType.Topic);

var slowChannel = rapidMq.CreateChannel(new ChannelConfig("MySlowChannel", 5, true));

var alertQueueBinding = rapidMq.CreateQueueBinding(
    new QueueModel(queue, true, false),
    exchange,
    routingKey
);

slowChannel.Listen(alertQueueBinding, new AlertNotifiedEventHandler()); // you can inject these type with DI too


var singleMessageChannel = rapidMq.CreateChannel(new ChannelConfig("SingleMessage", 1, true));

var iotNotificationsBinding = rapidMq.CreateQueueBinding(
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

//rapidMq.PublishMessage(exchange, routingKey, alertMessage);

Console.ReadLine();