namespace RapidMQ.Models;

public interface IMqMessage
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
}