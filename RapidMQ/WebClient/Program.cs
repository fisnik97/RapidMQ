using RapidMQ.Contracts;
using RapidMQ.Extensions;
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

builder.Services.AddLogging(x =>
{
    x.AddConsole()
        .AddDebug()
        .AddConfiguration(builder.Configuration.GetSection("Logging"));
});

// some random service
builder.Services.AddTransient<ISomeService, SomeService>();

builder.Services.AddSingleton<CancellationTokenSource>();

// Register RapidMQ using the simplified DI extension.
// Connection URI and retry settings are all that's needed for basic setup.
builder.Services.AddRapidMq(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var eventBusConnectionString = configuration.GetValue<string>("EventBusConnectionString")
                                   ?? throw new InvalidOperationException(
                                       "Please provide 'EventBusConnectionString' in configuration.");

    return new RapidMqOptions
    {
        ConnectionUri = new Uri(eventBusConnectionString),
        ConnectionManagerConfig = new ConnectionManagerConfig(
            maxMillisecondsDelay: 30000,
            initialMillisecondsRetry: 2000)
    };
});

// Register event handlers (used by the handler-class approach)
builder.Services.AddScoped<IMqMessageHandler<AlertReceivedEvent>, AlertReceivedEventHandler>();

// Register event bus
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