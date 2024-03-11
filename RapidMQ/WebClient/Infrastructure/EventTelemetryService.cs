using Microsoft.ApplicationInsights;
using RapidMQ.Contracts;

namespace WebClient.Infrastructure;

public class EventTelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;

    public EventTelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
        _telemetryClient.Context.Cloud.RoleName = "web.client.rabbitmq";
    }

    public void TrackPublish(string eventName, IDictionary<string, string> properties)
    {
        _telemetryClient.TrackEvent(eventName, properties);
    }

    public void TrackEvent(Guid operationId, string eventName, IDictionary<string, string> properties)
    {
        _telemetryClient.Context.Operation.Id = operationId.ToString();
        _telemetryClient.Context.Operation.Name = "EventBusEvent";
        _telemetryClient.TrackEvent(eventName, properties);
    }
}