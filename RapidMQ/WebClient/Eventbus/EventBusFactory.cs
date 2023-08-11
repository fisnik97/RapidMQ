using Microsoft.Extensions.Logging.Abstractions;
using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.EventHandlers;
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

        var alertReceivedQueue = rapidMq.DeclareQueue("alert.received.queue", true, false);
        
        var alertQueueBinding = new QueueBinding(alertReceivedQueue, "alert.received", iotExchange);

        var alertProcessingChannel = rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 1, true));
        
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
        alertProcessingChannel.Listen(alertQueueBinding, handler);
    }
}