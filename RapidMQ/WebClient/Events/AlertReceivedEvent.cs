using RapidMQ.Models;

namespace WebClient.Events;


[MqEventRoutingKey("alert.received")]
public class AlertReceivedEvent : MqMessage
{
    public string Name { get; set; }
    public int AlertSeverity { get; set; }
    public int[] ImpactedUsers { get; set; }
    public string AlertCause { get; set; }

    public override string ToString()
    {
        return
            $"Name: {Name}, AlertSeverity: {AlertSeverity}, ImpactedUsers: {string.Join(",", ImpactedUsers)}, AlertCause: {AlertCause}";
    }
}