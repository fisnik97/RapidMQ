using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RapidMQ.Contracts;
using RapidMQ.Internals;

namespace RapidMQ;

public class ConnectionManager : IConnectionManager
{
    public async Task<IConnection> ConnectAsync(Uri uri)
    {
        var policy = PolicyProvider.GetBackOffRetryPolicy<BrokerUnreachableException>(5, 2,
            onRetry: ((exception, span, attemptNr) =>
            {
                Console.WriteLine(
                    $"Could not connect to the RabbitMQ server after {attemptNr}(s) attempt, timespan: {span}!" +
                    $" - {exception.Message}");
            }));

        return await policy.ExecuteAsync(async token =>
        {
            return await Task.Run(() => new
                ConnectionFactory
                {
                    Uri = uri,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                }.CreateConnection(), token);
        }, CancellationToken.None);
    }
}