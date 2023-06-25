using RabbitMQ.Client;
using RapidMQ;
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

slowChannel.Listen(alertQueueBinding, () => { Console.WriteLine("Hello from event!"); });

slowChannel.Listen(iotNotificationsBinding, () => { Console.WriteLine("Hello from IoT notifications binding!"); });


Console.ReadLine();