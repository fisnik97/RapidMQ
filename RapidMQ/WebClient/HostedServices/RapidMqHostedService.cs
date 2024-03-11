using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Events;

namespace WebClient.HostedServices;

public class RapidMqHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public RapidMqHostedService(IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
    {
        _serviceProvider = serviceProvider;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var rapidMq = _serviceProvider.GetRequiredService<IRapidMq>();

        // define exchanges
        var iotExchange = rapidMq.GetOrCreateExchange("IoT", "topic");
        var alertReceivedQueue = rapidMq.DeclareQueue("alert.received.queue");

        
        // define channels
        var channelForAlertProcessing =
            rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300, true));
        var channelForNotifications = rapidMq.CreateRapidChannel(new ChannelConfig("notificationChannel", 1));
        
        // bindings
        var alertQueueBinding = rapidMq.GetOrCreateQueueBinding(alertReceivedQueue, iotExchange, "alert.received");
        var notificationBinding = rapidMq.GetOrCreateQueueBinding(new QueueModel("notifications.queue", true, false),
            iotExchange, "notification.received");
        
        // listen to queues
        using var scope = _serviceProvider.CreateScope();
        var alertHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
        channelForAlertProcessing.Listen(alertQueueBinding, alertHandler);

        var notificationHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<NotificationEvent>>();
        channelForNotifications.Listen(notificationBinding, notificationHandler);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}