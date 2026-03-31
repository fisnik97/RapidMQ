# RapidMQ - Simplified Customizable RabbitMQ Client Library

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-yellow.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET C#](https://img.shields.io/badge/.NET-C%23-green)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Nuget](https://img.shields.io/nuget/v/RapidMq?label=NuGet%20Component%20Library)](https://www.nuget.org/packages/RapidMq/)
[![NuGet](https://img.shields.io/nuget/dt/RapidMq?label=NuGet%20Downloads)](https://www.nuget.org/packages/RapidMq/)


## Introduction

`RapidMQ` is a simplified wrapper of [RabbitMQ](https://github.com/rabbitmq/rabbitmq-dotnet-client) library, designed to help developers manage [RabbitMQ](https://github.com/rabbitmq/rabbitmq-dotnet-client) interactions more easily, particularly by providing easy ways to configure channels with different attributes such as `prefetch count`, `prefetch size` and other settings. 
Library is designed to work with the so called `RapidChannels` which are wrappers of `IModel` interface. 
`RapidChannels` are designed to be used in a way that each channel is responsible it's routing the consumed messages to the appropriate client handler.
This way, developers can easily manage their queues and their interactions with RabbitMQ based on the channel configurations.

## Key Features
- Simple interface to RabbitMQ (_Requires basic knowledge of RabbitMQ_)
- Distinct handling of channels based on channel configurations
- Easy queue binding and setup
- In-built retry mechanisms for connection stability
- A design focusing on delivering messages effectively and consistently
- Message handlers contain more context about the message, not only the message body
- Support dependency injection for message handlers and other components
- **Flexible message handling**: use either handler classes (`IMqMessageHandler<T>`) or inline lambda callbacks
- **Simplified DI registration** via `AddRapidMq()` extension method

## Getting Started
```shell 
dotnet add package RapidMq
```

### Registering RapidMQ with Dependency Injection

The `AddRapidMq()` extension method handles all the boilerplate of setting up the connection manager, factory, and connection:

```csharp
using RapidMQ.Extensions;
using RapidMQ.Models;

builder.Services.AddRapidMq(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetValue<string>("EventBusConnectionString");

    return new RapidMqOptions
    {
        ConnectionUri = new Uri(connectionString),
        ConnectionManagerConfig = new ConnectionManagerConfig(
            maxMillisecondsDelay: 30000,
            initialMillisecondsRetry: 2000)
    };
});
```

### Setting up Channels and Bindings

```csharp 
var rapidMq = serviceProvider.GetRequiredService<IRapidMq>();

var iotExchange = rapidMq.GetOrCreateExchange("IoT", "topic");
var alertQueueBinding = rapidMq.GetOrCreateQueueBinding("alert.received.queue", iotExchange, "alert.received");

var alertProcessingChannel = rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300));
```

### Listening for Messages

RapidMQ supports two approaches for handling messages:

#### Approach 1: Handler Class (`IMqMessageHandler<T>`)

Best for handlers with injected dependencies or complex logic:

```csharp
public class AlertReceivedEventHandler : IMqMessageHandler<AlertReceivedEvent>
{
    private readonly ISomeService _someService;

    public AlertReceivedEventHandler(ISomeService someService)
    {
        _someService = someService;
    }

    public async Task Handle(MessageContext<AlertReceivedEvent> context)
    {
        Console.WriteLine($"Processing alert: {context.Message.Name}");
        await _someService.DoSomethingAsync();
    }
}

// Register and listen
var handler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();
alertProcessingChannel.Listen(alertQueueBinding, handler);
```

#### Approach 2: Inline Lambda Callback

Best for simple handlers that can be expressed in a few lines:

```csharp
notificationChannel.Listen<NotificationEvent>(notificationBinding, context =>
{
    var notification = context.Message;
    Console.WriteLine($"Notification received: {notification.NotificationId}");
    return Task.CompletedTask;
});
```

### Publishing Messages

```csharp
_rapidMq.PublishMessage("IoT", "alert.received", alertReceivedEvent);
```

For more details on setting up and configuring the library, please refer to the [.NetCoreAPI Example](https://github.com/fisnik97/RapidMQ/tree/main/RapidMQ/WebClient)