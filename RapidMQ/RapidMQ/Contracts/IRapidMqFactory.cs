using System.Text.Json;
using RapidMQ.Models;

namespace RapidMQ.Contracts;

public interface IRapidMqFactory
{
    Task<RapidMq> CreateAsync(ConnectionManagerConfig connectionManagerConfig,
        JsonSerializerOptions? jsonSerializerOptions = null);
}