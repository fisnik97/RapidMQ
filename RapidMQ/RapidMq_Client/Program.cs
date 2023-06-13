using RabbitMQ.Client;
using RapidMQ;

const string connString = "amqp://localhost";

var rapidMq = new RapidMq(connString, new ChannelConfig
{
    Channels = new List<RapidChannel>()
    {
        new RapidChannel(null, "SlowConsumeChannel", 100, false),
        new RapidChannel(null, "SlowConsumeChannel", 100, false),
        new RapidChannel(null, "SlowConsumeChannel", 100, false),
    }
});

const string queueName = "deviceAlertQueue", exchangeName = "IoT", routingKey = "device.iot.alert";

rapidMq.StartListening();

rapidMq.DeclareExchange(exchangeName, ExchangeType.Topic);

rapidMq.DeclareQueue(queueName, true, true);

rapidMq.BindQueue(queueName, routingKey, exchangeName);


