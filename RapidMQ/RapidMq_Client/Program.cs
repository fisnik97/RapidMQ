using RabbitMQ.Client;
using RapidMQ;

const string connString = "amqp://localhost";

var rapidMq = new RapidMq(connString, new List<ChannelConfig>()
{
    new("MetricsChannel", 1, true),
    new("DeviceChannel", 10, false),
});

const string queue = "alert.queue";
const string exchange = "IoT";
const string routingKey = "device.iot.alert";

var binding = rapidMq.CreateBinding(
    new QueueModel(queue, true, false),
    exchange,
    routingKey
);

rapidMq.Listen("MetricsChannel", binding,
    () =>
    {
        Console.WriteLine($"this is the handler of the event: {routingKey}");
    });

rapidMq.Listen("DeviceChannel",
    rapidMq.CreateBinding(QueueModel.DefaultQueue("SecondIoTQueue"), "IoT", "device.iot.message"),
    () =>
    {
        Console.WriteLine(""); 
        
    });

/*
const string queueName = "deviceAlertQueue", exchangeName = "IoT", routingKey = "device.iot.alert";

rapidMq.BindEventHandlers();

rapidMq.DeclareExchange(exchangeName, ExchangeType.Topic);

rapidMq.DeclareQueue(queueName, true, false);

rapidMq.BindQueue(queueName, routingKey, exchangeName);
*/

Console.ReadLine();