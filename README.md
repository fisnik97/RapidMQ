# RapidMQ - Simplified Customizable RabbitMQ Client Library

[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-yellow.svg)](https://opensource.org/licenses/Apache-2.0)
[![.NET C#](https://img.shields.io/badge/.NET-C%23-green)](https://docs.microsoft.com/en-us/dotnet/csharp/)
![Nuget](https://img.shields.io/nuget/v/RapidMq?label=NuGet%20Component%20Library)

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

## Getting Started
```shell 
dotnet add package RapidMq
```

### A simple view on using the library to setup a channel
```csharp 
IConnectionManager connectionManager = new ConnectionManager(logger);

var factory = new RapidMqConnectionFactory(connectionManager, logger);

var rapidMq = factory.CreateAsync(new Uri("amqp://localhost"), new ConnectionManagerSettings(...));

rapidMq.GetOrCreateExchange("IoT", "topic");
var alertReceivedQueue = rapidMq.DeclareQueue("alert.received.queue");

var alertQueueBinding = rapidMq.GetOrCreateQueueBinding(alertReceivedQueue, iotExchange, "alert.received");

// setting up the channels
var alertProcessingChannel = rapidMq.CreateRapidChannel(new ChannelConfig("alertProcessingChannel", 300));

using var scope = _serviceProvider.CreateScope();
var alertHandler = scope.ServiceProvider.GetRequiredService<IMqMessageHandler<AlertReceivedEvent>>();

// setting up the channel listeners
alertProcessingChannel.Listen(alertQueueBinding, alertHandler);

```

### A simple view on creating a message handler
```csharp

##Defining the message format
[MqEventRoutingKey("alert.received")]
public class AlertReceivedEvent : MqMessage
{
    public string Name { get; set; }
    public int AlertSeverity { get; set; }
}

// Defining the message handler
public class AlertReceivedEventHandler : IMqMessageHandler<AlertReceivedEvent>
{
    public async Task Handle(MessageContext<AlertReceivedEvent> context)
    {
        Console.WriteLine($"Processing event with payload: {context.Message}");

        await _someService.DoSomethingAsync();

        Console.WriteLine(
            $"Processing event with payload: {context.Message} and routingKey: {context.RoutingKey} completed");
    }
}

```

## A simple view on using the library to publish a message
```csharp

public class SomeService 
{
    private readonly IRapidMq _rapidMq;
    
    public SomeService(IRapidMq rapidMq)
    {
        _rapidMq = rapidMq;
    }
    
    public void PublishAlertReceivedEventAsync()
    {
        var @alertReceivedEvent = new AlertReceivedEvent
        {
            Name = "Alert 1",
            AlertSeverity = 1
        };

        _rapidMq.PublishMessage(exchangeName, routingKey, @alertReceivedEvent);
    }
}

```

For more details on setting up and configuring the library, please refer to the [.NetCoreAPI Example](https://github.com/fisnik97/RapidMQ/tree/main/RapidMQ/WebClient)

## Status
RapidMQ is deployed in the initial version 1.0.0. Next objective is to add support for more advanced features such as: introducing more ways to handle the message, especially when the message is not processed successfully!

## Contact
For any suggestions, questions, or feedback, please reach out: 
- [Email](mailto:ffisnikmaloku@gmail.com)
- [LinkedIn](https://www.linkedin.com/in/fisnik-maloku-992b26141/)

## Contributions
If you would like to contribute to the development of RapidMQ, please feel free to fork this repository and submit pull requests.
