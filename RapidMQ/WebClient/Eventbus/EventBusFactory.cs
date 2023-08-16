using Microsoft.Extensions.Logging.Abstractions;
using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Events;

namespace WebClient.Eventbus;

public static class EventBusFactory
{
    public static async Task<IRapidMq> InstantiateEventBus(this WebApplicationBuilder app)
    {
        var connString = app.Configuration.GetValue<string>("EventBusConnectionString");

        if (connString == null)
            throw new ArgumentNullException(nameof(app), "Eventbus connection string is missing!");

        var connectionManager = new ConnectionManager
        {
            OnConnectionDrop = args =>
            {
                Console.WriteLine($"Connection dropped because {args.Cause}, reconnecting...");
                return Task.CompletedTask;
            },
        };

        var rapidMqLogger = new Logger<IRapidMq>(new NullLoggerFactory());

        var rapidMqFactory = new RapidMqFactory(connectionManager, rapidMqLogger);

        var rapidMq = await rapidMqFactory.CreateAsync(new Uri(connString));

        if (rapidMq == null)
            throw new ArgumentNullException(nameof(rapidMq), "RapidMQ is null!");

        return rapidMq;
    }

    public static void BuildInfrastructure(this IRapidMq rapidMq, IServiceProvider serviceProvider)
    {
        var iotExchange = rapidMq.GetOrCreateExchange("IoT", "topic");
        var alertReceivedQueue = rapidMq.DeclareQueue("alert.received.queue");

        var alertQueueBinding = rapidMq.GetOrCreateQueueBinding(alertReceivedQueue, iotExchange, "alert.received");
        var notificationBinding = rapidMq.GetOrCreateQueueBinding(new QueueModel("notifications.queue", true, false),
            iotExchange, "notification.received");


        var alertProcessingChannel = rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300));
        var notificationChannel = rapidMq.CreateRapidChannel(new ChannelConfig("notificationChannel", 1));

        using var scope = serviceProvider.CreateScope();
        var alertHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
        alertProcessingChannel.Listen(alertQueueBinding, alertHandler);

        var notificationHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<NotificationEvent>>();
        notificationChannel.Listen(notificationBinding, notificationHandler);
    }
}