namespace RapidMQ.Models;

public record EventBinding(string RoutingKey, Action Handler);