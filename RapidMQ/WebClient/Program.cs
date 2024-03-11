using Microsoft.ApplicationInsights.Extensibility;
using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Eventbus;
using WebClient.EventHandlers;
using WebClient.Events;
using WebClient.HostedServices;
using WebClient.Infrastructure;
using WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(x =>
{
    x.AddConsole()
        .AddDebug()
        .AddConfiguration(builder.Configuration.GetSection("Logging"));
});


// some random service
builder.Services.AddTransient<ISomeService, SomeService>();

builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<ILogger, Logger<IRapidMq>>();

builder.Services.AddSingleton<CancellationTokenSource>();

builder.Services.AddSingleton<IRapidMq>(sp =>
{
    // Resolve dependencies
    var connectionManager = sp.GetRequiredService<IConnectionManager>();
    var eventTelemetry = sp.GetRequiredService<ITelemetryService>();
    var cancellationTokenSource = sp.GetRequiredService<CancellationTokenSource>();

    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<IRapidMq>();

    var configuration = sp.GetRequiredService<IConfiguration>();
    var eventBusConnectionString = configuration.GetValue<string>("EventBusConnectionString");

    if (eventBusConnectionString == null)
        throw new ArgumentNullException(nameof(configuration), "Please provide a connection for amqp!");

    // These configurations can also be read from appsettings.json using a section
    const int maxMillisecondsRetry = 30000;
    const int initialMillisecondsRetry = 2000;

    var connectionManagerConfig =
            new ConnectionManagerConfig(
                maxMillisecondsRetry,
                initialMillisecondsRetry)
            {
                OnConnection = () =>
                {
                    logger.LogInformation("Client has been connected to the broker!");
                    return Task.CompletedTask;
                },
                OnConnectionShutdownEventHandler = (args) =>
                {
                    logger.LogError("Client has been disconnected from the broker! Reason: {0}, Cause: {1} ",
                        args.ReplyText, args.Cause);
                    return Task.CompletedTask;
                }
            }
        ;

    var rapidMqFactory = new RapidMqFactory(connectionManager, logger, eventTelemetry);

    return
        rapidMqFactory
            .CreateAsync(new Uri(eventBusConnectionString), connectionManagerConfig, cancellationTokenSource.Token)
            .GetAwaiter()
            .GetResult();
});


// register event handlers
builder.Services.AddScoped<IMqMessageHandler<AlertReceivedEvent>, AlertReceivedEventHandler>();
builder.Services.AddScoped<IMqMessageHandler<NotificationEvent>, NotificationEventHandler>();

// register event bus
builder.Services.AddSingleton<IEventBus, EventBus>();

builder.Services.AddHostedService<RapidMqHostedService>();

// app insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

builder.Services.AddTransient<ITelemetryService, EventTelemetryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();