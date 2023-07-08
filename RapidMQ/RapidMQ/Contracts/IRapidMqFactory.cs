namespace RapidMQ;

public interface IRapidMqFactory
{
    Task<RapidMq> CreateAsync(Uri connectionUri);
}