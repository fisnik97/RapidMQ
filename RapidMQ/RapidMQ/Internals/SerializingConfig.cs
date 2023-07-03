using System.Text.Json;
using System.Text.Json.Serialization;

namespace RapidMQ.Internals;

internal static class SerializingConfig
{
    internal static JsonSerializerOptions DefaultOptions =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}