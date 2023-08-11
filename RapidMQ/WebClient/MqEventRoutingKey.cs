namespace WebClient;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class MqEventRoutingKey : Attribute
{
    public string RoutingKey { get; set; }

    public MqEventRoutingKey(string routingKey)
    {
        RoutingKey = routingKey;
    }
}