using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RapidMQ.Models;
using Xunit;

namespace RapidMq.Unit.Tests.ConnectionManager.Spec;

public class ConnectionManagerTests
{
    /*
    private readonly Mock<ILogger<RapidMQ.ConnectionManager>> _mockLogger;
    private readonly TestableConnectionManager _testableConnectionManager;
    private readonly Mock<IConnection> _mockConnection;

    private const string RabbitMqUri = "amqp://localhost";

    public ConnectionManagerTests()
    {
        _mockLogger = new Mock<ILogger<RapidMQ.ConnectionManager>>();
        _mockConnection = new Mock<IConnection>();
        _testableConnectionManager = new TestableConnectionManager(_mockLogger.Object);
    }

    [Fact]
    public async Task ConnectionManager_Should_Return_ConnectionInstance()
    {
        var config = new ConnectionManagerConfig(3, TimeSpan.FromMilliseconds(100), false);

        var mockConnection = new Mock<IConnection>();
        _testableConnectionManager.CreateConnectionInstanceMock = uri => mockConnection.Object;

        var connection = await _testableConnectionManager.ConnectAsync(new Uri(RabbitMqUri), config);

        Assert.NotNull(connection);
        Assert.Same(mockConnection.Object, connection);
    }


    [Fact]
    public async Task ConnectionManager_Should_Retry_Until_Successful()
    {
        var config = new ConnectionManagerConfig(3, TimeSpan.FromMilliseconds(100), false);

        _testableConnectionManager.FailuresToSimulate = 2;

        var connection = await _testableConnectionManager.ConnectAsync(new Uri(RabbitMqUri), config);

        Assert.NotNull(connection);
    }
    */
}