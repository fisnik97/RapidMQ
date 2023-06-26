using RabbitMQ.Client;
using RapidMQ;
using RapidMq_Client;
using RapidMQ.Models;

const string connString = "amqp://localhost";

var rapidMq = new RapidMq(new Uri(connString));


const string queue = "alert.queue";
const string notificationsQueue = "alert.notifications.queue";
const string exchange = "IoT";
const string routingKey = "device.iot.alert", notificationsRoutingKey = "device.notifications.received";

rapidMq.DeclareExchange(exchange, ExchangeType.Topic);


var slowChannel = rapidMq.CreateChannel(new ChannelConfig("MySlowChannel", 5, true));

var alertQueueBinding = rapidMq.CreateQueueBinding(
    new QueueModel(queue, true, false),
    exchange,
    routingKey
);

var iotNotificationsBinding = rapidMq.CreateQueueBinding(
    notificationsQueue,
    exchange,
    notificationsRoutingKey
);

slowChannel.Listen(alertQueueBinding, new AlertNotifiedEventHandler()); // you can inject these type with DI too

slowChannel.Listen(iotNotificationsBinding, new AlertNotifiedEventHandler());


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