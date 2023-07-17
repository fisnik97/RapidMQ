using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RapidMQ.Contracts;
using RapidMQ.Internals;

namespace RapidMQ;

public class ConnectionManager : IConnectionManager
{
    public Func<ShutdownEventArgs, Task>? OnConnectionDrop { get; set; }

    public async Task<IConnection> ConnectAsync(Uri uri)
    {
        var policy = PolicyProvider.GetBackOffRetryPolicy<BrokerUnreachableException>(5, 2,
            onRetry: ((exception, span, attemptNr) =>
            {
                Console.WriteLine(
                    $"Could not connect to the RabbitMQ server after {attemptNr}(s) attempt, timespan: {span}!" +
                    $" - {exception.Message}");
            }));

        var connection = await policy.ExecuteAsync(async token =>
        {
            return await Task.Run(() => new
                ConnectionFactory
                {
                    Uri = uri,
                    RequestedHeartbeat = TimeSpan.FromSeconds(30),
                }.CreateConnection(), token);
        }, CancellationToken.None);

        if (OnConnectionDrop != null)
            connection.ConnectionShutdown += (_, args) => { Task.Run(() => { OnConnectionDrop(args); }); };

        return connection;
    }
}