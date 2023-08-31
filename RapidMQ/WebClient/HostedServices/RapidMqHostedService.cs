using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Events;

namespace WebClient.HostedServices;

public class RapidMqHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public RapidMqHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var rapidMq = _serviceProvider.GetRequiredService<IRapidMq>();

        var iotExchange = rapidMq.GetOrCreateExchange("IoT", "topic");
        var alertReceivedQueue = rapidMq.DeclareQueue("alert.received.queue");

        var alertQueueBinding = rapidMq.GetOrCreateQueueBinding(alertReceivedQueue, iotExchange, "alert.received");
        var notificationBinding = rapidMq.GetOrCreateQueueBinding(new QueueModel("notifications.queue", true, false),
            iotExchange, "notification.received");


        var alertProcessingChannel = rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300));
        var notificationChannel = rapidMq.CreateRapidChannel(new ChannelConfig("notificationChannel", 1));

        using var scope = _serviceProvider.CreateScope();
        var alertHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
        alertProcessingChannel.Listen(alertQueueBinding, alertHandler);

        var notificationHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<NotificationEvent>>();
        notificationChannel.Listen(notificationBinding, notificationHandler);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}