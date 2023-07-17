namespace RapidMQ.Contracts;

public interface IRapidMqFactory
{
    Task<IRapidMq> CreateAsync(Uri connectionUri);
}