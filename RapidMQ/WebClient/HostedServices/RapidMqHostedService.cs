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

        // Declare the exchange and queue bindings
        var iotExchange = rapidMq.GetOrCreateExchange("IoT", "topic");

        var alertQueueBinding = rapidMq.GetOrCreateQueueBinding("alert.received.queue", iotExchange, "alert.received");
        var notificationBinding = rapidMq.GetOrCreateQueueBinding(
            new QueueModel("notifications.queue", true, false), iotExchange, "notification.received");

        // --- Approach 1: Handler class (IMqMessageHandler<T>) ---
        // Use this when your handler has dependencies (e.g. injected services) or complex logic.
        var alertProcessingChannel =
            rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300, true));

        using var scope = _serviceProvider.CreateScope();
        var alertHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
        alertProcessingChannel.Listen(alertQueueBinding, alertHandler);

        // --- Approach 2: Inline lambda callback (Func<MessageContext<T>, Task>) ---
        // Use this for simple handlers that don't need DI or can be expressed in a few lines.
        var notificationChannel = rapidMq.CreateRapidChannel(new ChannelConfig("notificationChannel", 1));

        notificationChannel.Listen<NotificationEvent>(notificationBinding, async context =>
        {
            var notification = context.Message;
            Console.WriteLine($"Notification received: {notification.NotificationId} via routing key: {context.RoutingKey}");
            await Task.CompletedTask;
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}