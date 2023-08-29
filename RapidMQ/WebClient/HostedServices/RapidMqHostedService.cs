using RapidMQ;
using RapidMQ.Contracts;
using RapidMQ.Models;
using WebClient.Eventbus;

namespace WebClient.HostedServices;

public class RapidMqHostedService : IHostedService
{
    private IConfiguration _configuration;
    private readonly IConnectionManager _connectionManager;
    private readonly Logger<IRapidMq> _logger;
    private IRapidMq? _rapidMq;
    private readonly IServiceProvider _serviceProvider;

    public RapidMqHostedService(IConfiguration configuration, IConnectionManager connectionManager,
        Logger<IRapidMq> logger, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _connectionManager = connectionManager;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connString = _configuration.GetValue<string>("EventBusConnectionString");
        if (connString == null)
            throw new ArgumentNullException(nameof(_configuration), "Eventbus connection string is missing!");

        var connectionManagerConfig =
            new ConnectionManagerConfig(new Uri(connString), 5, TimeSpan.FromSeconds(100), true);

        var rapidMqFactory = new RapidMqFactory(_connectionManager, _logger);

        _rapidMq = await rapidMqFactory.CreateAsync(connectionManagerConfig);

        if (_rapidMq == null)
            throw new ArgumentNullException(nameof(_rapidMq), "RapidMQ is null!");

        _rapidMq.SetUpInfrastructure(_serviceProvider);
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}