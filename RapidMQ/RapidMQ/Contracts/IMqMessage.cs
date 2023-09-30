namespace RapidMQ.Contracts;

/// <summary>
/// Base interface for all messages sent through RapidMQ
/// </summary>
public interface IMqMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
}