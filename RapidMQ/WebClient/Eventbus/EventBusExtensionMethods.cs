using Microsoft.Extensions.Logging.Abstractions;
using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Events;

namespace WebClient.Eventbus;

public static class EventBusExtensionMethods
{
    public static void SetUpInfrastructure(this IRapidMq rapidMq, IServiceProvider serviceProvider)
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