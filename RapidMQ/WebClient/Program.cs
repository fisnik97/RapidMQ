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

// some random service
builder.Services.AddTransient<ISomeService, SomeService>();

builder.Services.AddLogging();

builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

// register event handlers
builder.Services.AddScoped<IMqMessageHandler<AlertReceivedEvent>, AlertReceivedEventHandler>();
builder.Services.AddScoped<IMqMessageHandler<NotificationEvent>, NotificationEventHandler>();


// register event bus
builder.Services.AddSingleton<IEventBus, EventBus>();

// Hosted service
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