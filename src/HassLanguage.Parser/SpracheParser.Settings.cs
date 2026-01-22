using Sprache;
using HassLanguage.Core.Ast;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
    // Settings parsers
    private static Parser<Settings> Settings =>
        Token("settings").Then(_ =>
            Token("{").Then(_ =>
                EventsSettings.Optional().Then(events =>
                    Token("}").Return(new Settings 
                    { 
                        Events = events.IsDefined ? events.Get() : null 
                    }))));

    private static Parser<EventsSettings> EventsSettings =>
        Token("events").Then(_ =>
            Token("{").Then(_ =>
                (Token("mode").Then(_ => 
                    Token(":").Then(_ => 
                        Identifier.Or(StringLiteral).Then(mode =>
                            Token(";").Return(mode)))).Optional()).Then(mode =>
                    RabbitMqSettings.Optional().Then(rabbitmq =>
                        Token("}").Return(new EventsSettings 
                        { 
                            Mode = mode.IsDefined ? mode.Get() : "auto",
                            RabbitMq = rabbitmq.IsDefined ? rabbitmq.Get() : null 
                        })))));

    private static Parser<RabbitMqSettings> RabbitMqSettings =>
        Token("rabbitmq").Then(_ =>
            Token("{").Then(_ =>
                RabbitMqProperty.Many().Select(props => {
                    var dict = props.ToDictionary(p => p.Key, p => p.Value);
                    return new RabbitMqSettings 
                    { 
                        Url = dict.GetValueOrDefault("url") as Expression ?? null!,
                        Exchange = dict.GetValueOrDefault("exchange") as string ?? string.Empty,
                        QueuePrefix = dict.GetValueOrDefault("queue_prefix") as string ?? string.Empty,
                        RoutingKey = dict.GetValueOrDefault("routing_key") as string ?? string.Empty,
                        PublishFromWebsocket = dict.GetValueOrDefault("publish_from_websocket") as bool? ?? false,
                        PublisherRole = dict.GetValueOrDefault("publisher_role") as Expression ?? null!,
                        PublisherEnabled = dict.GetValueOrDefault("publisher_enabled") as Expression ?? null!
                    };
                }).Contained(SkipWhitespace, Token("}"))));

    private static Parser<(string Key, object? Value)> RabbitMqProperty =>
        Token("url").Then(_ => 
            Token(":").Then(_ => 
                Expression.Then(url =>
                    Token(";").Return(("url", (object?)url)))))
        .Or(Token("exchange").Then(_ => 
            Token(":").Then(_ => 
                StringLiteral.Then(exchange =>
                    Token(";").Return(("exchange", (object?)exchange)))))
        .Or(Token("queue_prefix").Then(_ => 
            Token(":").Then(_ => 
                StringLiteral.Then(prefix =>
                    Token(";").Return(("queue_prefix", (object?)prefix)))))
        .Or(Token("routing_key").Then(_ => 
            Token(":").Then(_ => 
                StringLiteral.Then(key =>
                    Token(";").Return(("routing_key", (object?)key)))))
        .Or(Token("publish_from_websocket").Then(_ => 
            Token(":").Then(_ => 
                BooleanLiteral.Then(pub =>
                    Token(";").Return(("publish_from_websocket", (object?)pub)))))
        .Or(Token("publisher_role").Then(_ => 
            Token(":").Then(_ => 
                Expression.Then(role =>
                    Token(";").Return(("publisher_role", (object?)role)))))
        .Or(Token("publisher_enabled").Then(_ => 
            Token(":").Then(_ => 
                Expression.Then(enabled =>
                    Token(";").Return(("publisher_enabled", (object?)enabled)))));
}
