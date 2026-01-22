namespace HassLanguage.Core.Ast;

public class Settings : IAstNode
{
    public EventsSettings? Events { get; set; }
}

public class EventsSettings : IAstNode
{
    public string Mode { get; set; } = "auto"; // auto | websocket | rabbitmq
    public RabbitMqSettings? RabbitMq { get; set; }
}

public class RabbitMqSettings : IAstNode
{
    public Expression Url { get; set; } = null!;
    public string Exchange { get; set; } = string.Empty;
    public string QueuePrefix { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public bool PublishFromWebsocket { get; set; }
    public Expression PublisherRole { get; set; } = null!;
    public Expression PublisherEnabled { get; set; } = null!;
}
