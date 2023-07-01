using RapidMQ.Models;

namespace RapidMq_Client;

public class NotificationEvent : MqMessage
{
    public int NotificationId { get; set; }
    public string Name { get; set; }
    public int[] MemberIds { get; set; }
}