using RabbitMQ.Client;
using RapidMQ.Internals;

namespace RapidMQ;

public interface IConnectionManager
{
    Task<IConnection> ConnectAsync(Uri uri);
}

public class ConnectionManager : IConnectionManager
{
    public async Task<IConnection> ConnectAsync(Uri uri)
    {
        var policy = PolicyProvider.GetBackOffRetryPolicy(5, 2,
            onRetry: ((exception, span, attemptNr) =>
            {
                Console.WriteLine(
                    $"Could not connect to the RabbitMQ server after {attemptNr}(s) attempt, timespan: {span}! - {exception.Message}");
            }));

        return await policy.ExecuteAsync(async token =>
        {
            return await Task.Run(() => new
                ConnectionFactory
                {
                    Uri = uri
                }.CreateConnection(), token);
        }, CancellationToken.None);
    }
}