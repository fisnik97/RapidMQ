using System.Text.Json;

namespace RapidMQ.Models;

/// <summary>
/// Configuration options for the AddRapidMq service extension.
/// </summary>
public class RapidMqOptions
{
    /// <summary>
    /// The RabbitMQ broker connection URI.
    /// </summary>
    public Uri ConnectionUri { get; set; }

    /// <summary>
    /// Connection retry and event configuration.
    /// </summary>
    public ConnectionManagerConfig ConnectionManagerConfig { get; set; }

    /// <summary>
    /// Optional custom JSON serializer options for message serialization/deserialization.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
}
