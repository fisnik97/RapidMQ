using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RapidMQ.Contracts;
using RapidMQ.Models;

namespace RapidMQ.Extensions;

/// <summary>
/// Extension methods for registering RapidMQ services in the dependency injection container.
/// </summary>
public static class RapidMqServiceExtensions
{
    /// <summary>
    /// Registers RapidMQ services and creates a configured <see cref="IRapidMq"/> singleton.
    /// This simplifies the setup by handling the connection manager, factory, and connection creation.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionUri">The RabbitMQ broker connection URI</param>
    /// <param name="connectionManagerConfig">Connection retry and event configuration</param>
    /// <param name="jsonSerializerOptions">Optional custom JSON serializer options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRapidMq(
        this IServiceCollection services,
        Uri connectionUri,
        ConnectionManagerConfig connectionManagerConfig,
        JsonSerializerOptions jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(connectionUri);
        ArgumentNullException.ThrowIfNull(connectionManagerConfig);

        services.AddSingleton<IConnectionManager, ConnectionManager>();

        services.AddSingleton<IRapidMq>(sp =>
        {
            var connectionManager = sp.GetRequiredService<IConnectionManager>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<IRapidMq>();

            var cancellationTokenSource = sp.GetService<CancellationTokenSource>() ?? new CancellationTokenSource();

            var factory = new RapidMqFactory(connectionManager, logger);

            // Note: GetAwaiter().GetResult() is used here because DI singleton factories are synchronous.
            // This is consistent with the existing RapidMqFactory usage pattern.
            return factory
                .CreateAsync(connectionUri, connectionManagerConfig, cancellationTokenSource.Token,
                    jsonSerializerOptions)
                .GetAwaiter()
                .GetResult();
        });

        return services;
    }

    /// <summary>
    /// Registers RapidMQ services using a configuration delegate for deferred setup.
    /// Useful when the connection URI or configuration depends on <see cref="IConfiguration"/> or other services.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">A delegate that receives the service provider and returns the RapidMQ options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRapidMq(
        this IServiceCollection services,
        Func<IServiceProvider, RapidMqOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddSingleton<IConnectionManager, ConnectionManager>();

        services.AddSingleton<IRapidMq>(sp =>
        {
            var options = configure(sp);
            ArgumentNullException.ThrowIfNull(options.ConnectionUri, nameof(options.ConnectionUri));
            ArgumentNullException.ThrowIfNull(options.ConnectionManagerConfig,
                nameof(options.ConnectionManagerConfig));

            var connectionManager = sp.GetRequiredService<IConnectionManager>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<IRapidMq>();

            var cancellationTokenSource = sp.GetService<CancellationTokenSource>() ?? new CancellationTokenSource();

            var factory = new RapidMqFactory(connectionManager, logger);

            // Note: GetAwaiter().GetResult() is used here because DI singleton factories are synchronous.
            // This is consistent with the existing RapidMqFactory usage pattern.
            return factory
                .CreateAsync(options.ConnectionUri, options.ConnectionManagerConfig,
                    cancellationTokenSource.Token, options.JsonSerializerOptions)
                .GetAwaiter()
                .GetResult();
        });

        return services;
    }
}
