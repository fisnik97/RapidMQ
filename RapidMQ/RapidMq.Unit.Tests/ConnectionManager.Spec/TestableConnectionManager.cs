using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

public class TestableConnectionManager : RapidMQ.ConnectionManager
{
    public int FailuresToSimulate { get; set; } = 0;
    private int _currentFailures = 0;
    public Func<Uri, IConnection> CreateConnectionInstanceMock { get; set; }

    public TestableConnectionManager(ILogger<RapidMQ.ConnectionManager> logger)
        : base(logger)
    {
    }

    protected override IConnection CreateConnectionInstance(Uri uri)
    {
        if (_currentFailures >= FailuresToSimulate)
            return CreateConnectionInstanceMock?.Invoke(uri) ?? base.CreateConnectionInstance(uri);
        _currentFailures++;
        throw new BrokerUnreachableException(new Exception("Simulated connection failure."));
    }
}