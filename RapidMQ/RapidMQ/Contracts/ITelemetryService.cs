namespace RapidMQ.Contracts;

public interface ITelemetryService
{
    public void TrackPublish(string eventName, IDictionary<string, string> properties);
    
    public void TrackEvent(Guid operationId, string eventName, IDictionary<string, string> properties);
}