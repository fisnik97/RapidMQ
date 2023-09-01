using System.Text.Json;
using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMqFactory
{
    /// <summary>
    /// Instantiates a new RapidMq instance.
    /// </summary>
    /// <param name="connectionUri">Uri of the RabbitMQ broker</param>
    /// <param name="connectionManagerConfig">Configuration for the rabbitMq connection</param>
    /// <param name="jsonSerializerOptions">Optional parameter for serializing and deserializing messages</param>
    /// <returns></returns>
    Task<RapidMq> CreateAsync(Uri connectionUri, ConnectionManagerConfig connectionManagerConfig,
        JsonSerializerOptions? jsonSerializerOptions = null);
}