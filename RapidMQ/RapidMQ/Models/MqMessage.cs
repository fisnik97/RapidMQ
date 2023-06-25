namespace RapidMQ.Models;

public class MqMessage : IMqMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}