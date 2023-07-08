namespace RapidMQ.Contracts;

public interface IRapidMqFactory
{
    Task<RapidMq> CreateAsync(Uri connectionUri);
}