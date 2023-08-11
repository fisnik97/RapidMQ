using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Eventbus;
using WebClient.EventHandlers;
using WebClient.Events;
using WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// some random service
builder.Services.AddTransient<ISomeService, SomeService>();


var rapidMq = await builder
    .InstantiateEventBus();


builder.Services.AddSingleton<IRapidMq>(rapidMq);

// register event handlers
builder.Services.AddScoped<IMqMessageHandler<AlertReceivedEvent>, AlertReceivedEventHandler>();

// register event bus
builder.Services.AddSingleton<IEventBus, EventBus>();

var app = builder.Build();

rapidMq.BuildInfrastructure(app.Services);
    
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