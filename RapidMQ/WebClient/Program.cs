using Microsoft.Extensions.Logging.Abstractions;
using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Eventbus;
using WebClient.EventHandlers;
using WebClient.Events;
using WebClient.HostedServices;
using WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();
// some random service
builder.Services.AddTransient<ISomeService, SomeService>();

builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<ILogger, Logger<IRapidMq>>();

builder.Services.AddSingleton<IRapidMq>(sp =>
{
    // Resolve dependencies
    var connectionManager = sp.GetRequiredService<IConnectionManager>();

    // ideally create a logger implementation for IRapidMq
    var logger = new Logger<IRapidMq>(new NullLoggerFactory());

    var configuration = sp.GetRequiredService<IConfiguration>();
    var eventBusConnectionString = configuration.GetValue<string>("EventBusConnectionString");

    if (eventBusConnectionString == null)
        throw new ArgumentNullException(nameof(configuration), "Please provide a connection for amqp!");

    // These configurations can also be read from appsettings.json using a section
    const int maxConnectionRetries = 5;
    const bool exponentialBackoffRetry = true;
    var connectionRetryDelay = TimeSpan.FromSeconds(5);

    var connectionManagerConfig =
        new ConnectionManagerConfig(maxConnectionRetries, connectionRetryDelay,
            exponentialBackoffRetry);
    var rapidMqFactory = new RapidMqFactory(connectionManager, logger);

    return
        rapidMqFactory.CreateAsync(new Uri(eventBusConnectionString), 
                connectionManagerConfig).GetAwaiter()
            .GetResult();
});


// register event handlers
builder.Services.AddScoped<IMqMessageHandler<AlertReceivedEvent>, AlertReceivedEventHandler>();
builder.Services.AddScoped<IMqMessageHandler<NotificationEvent>, NotificationEventHandler>();

// register event bus
builder.Services.AddSingleton<IEventBus, EventBus>();

builder.Services.AddHostedService<RapidMqHostedService>();

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