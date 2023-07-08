using RabbitMQ.Client;
using RapidMQ.Contracts;
using RapidMQ.Internals;

namespace RapidMQ;

public class ChannelFactory : IChannelFactory
{
    public async Task<IModel> CreateChannel(IConnection connection)
    {
        var policy = PolicyProvider.GetLinearRetryPolicy(5, 2, onRetry: ((exception, span, retryAttempt) =>
        {
            Console.WriteLine(
                $"Could not connect to the SetupChannel after ${retryAttempt}(s) attempt, timespan: {span}!" +
                $" - {exception.Message}");
        }));

        return await policy.ExecuteAsync(async token => await Task.Run(connection.CreateModel, token),
            CancellationToken.None);
    }
}