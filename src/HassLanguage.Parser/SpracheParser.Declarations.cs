using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Entity declarations
  private static Parser<EntityDeclaration> EntityDeclaration =>
    EntityType.Then(type =>
      Identifier.Then(alias =>
        Token("{")
          .Then(_ =>
            EntityProperty
              .Many()
              .Then(props =>
                Token("}")
                  .Return(
                    new EntityDeclaration
                    {
                      Type = type,
                      Alias = alias,
                      Properties = props.ToDictionary(p => p.Key, p => p.Value),
                    }
                  )
              )
          )
      )
    );

  private static Parser<string> EntityType =>
    Token("light")
      .Or(Token("binary_sensor"))
      .Or(Token("sensor"))
      .Or(Token("climate"))
      .Or(Token("switch"))
      .Or(Token("cover"))
      .Or(Identifier);

  private static Parser<(string Key, Expression Value)> EntityProperty =>
    Identifier.Then(key =>
      Token(":").Then(_ => Expression.Then(value => Token(";").Return((key, value))))
    );

  private static Parser<List<EntityDeclaration>> EntityList =>
    Token("entities")
      .Then(_ =>
        Token(":")
          .Then(_ =>
            Token("[")
              .Then(_ =>
                EntityDeclaration
                  .DelimitedBy(Sprache.Parse.Char(',').Contained(SkipWhitespace, SkipWhitespace))
                  .Contained(SkipWhitespace, SkipWhitespace)
                  .Then(entities => Token("]").Then(_ => Token(";").Return(entities.ToList())))
              )
          )
      );

  // Device declaration
  private static Parser<DeviceDeclaration> DeviceDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("device")
          .Then(_ =>
            StringLiteral.Then(displayName =>
              Identifier.Then(alias =>
                Identifier
                  .Optional()
                  .Then(type =>
                    Token("{")
                      .Then(_ =>
                        EntityList
                          .Optional()
                          .Then(entities =>
                            Token("}")
                              .Return(
                                new DeviceDeclaration
                                {
                                  DisplayName = displayName,
                                  Alias = alias,
                                  Type = type.IsDefined ? type.Get() : null,
                                  Decorators = decorators.IsDefined
                                    ? decorators.Get()
                                    : new List<Decorator>(),
                                  Entities = entities.IsDefined
                                    ? entities.Get()
                                    : new List<EntityDeclaration>(),
                                }
                              )
                          )
                      )
                  )
              )
            )
          )
      );

  // Room declaration
  private static Parser<RoomDeclaration> RoomDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("room")
          .Then(_ =>
            StringLiteral.Then(displayName =>
              Identifier.Then(alias =>
                Identifier
                  .Optional()
                  .Then(type =>
                    Token("{")
                      .Then(_ =>
                        DeviceDeclaration
                          .Many()
                          .Then(devices =>
                            Token("}")
                              .Return(
                                new RoomDeclaration
                                {
                                  DisplayName = displayName,
                                  Alias = alias,
                                  Type = type.IsDefined ? type.Get() : null,
                                  Decorators = decorators.IsDefined
                                    ? decorators.Get()
                                    : new List<Decorator>(),
                                  Devices = devices.ToList(),
                                }
                              )
                          )
                      )
                  )
              )
            )
          )
      );

  // Home declaration
  private static Parser<HomeDeclaration> HomeDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("home")
          .Then(_ =>
            StringLiteral.Then(displayName =>
              (
                Identifier.Or(SkipWhitespace.Many().Then(_ => Sprache.Parse.Return(string.Empty)))
              ).Then(alias =>
                Token("{")
                  .Then(_ =>
                    Settings
                      .Optional()
                      .Then(settings =>
                        RoomDeclaration
                          .Many()
                          .Then(rooms =>
                            Token("}")
                              .Return(
                                new HomeDeclaration
                                {
                                  DisplayName = displayName,
                                  Alias = alias,
                                  Decorators = decorators.IsDefined
                                    ? decorators.Get()
                                    : new List<Decorator>(),
                                  Settings = settings.IsDefined ? settings.Get() : null,
                                  Rooms = rooms.ToList(),
                                }
                              )
                          )
                      )
                  )
              )
            )
          )
      );
}
