namespace RapidMQ;

public record EventBinding(string RoutingKey, Action Handler);