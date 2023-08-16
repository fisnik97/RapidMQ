using RapidMQ.Models;

namespace WebClient.Events;

[MqEventRoutingKey("notification.received")]
public class NotificationEvent : MqMessage
{
    public int NotificationId { get; set; }
    public string? NotifiedFromSource { get; set; }
    public DateTime NotifiedDate { get; set; }
}