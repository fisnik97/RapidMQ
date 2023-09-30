using RapidMQ.Contracts;

namespace RapidMQ.Models;

/// <summary>
/// Base implementation of IMqMessage interface 
/// </summary>
public class MqMessage : IMqMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}