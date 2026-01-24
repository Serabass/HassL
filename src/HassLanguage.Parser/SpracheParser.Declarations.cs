using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Entity declarations
  // Format: binary_sensor motion = binary_sensor.kitchen_motion
  private static Parser<EntityDeclaration> EntityDeclaration =>
    EntityType.Then(type =>
      Identifier.Then(alias =>
        Token("=")
          .Then(_ =>
            Expression.Select(idExpr =>
              new EntityDeclaration
              {
                Type = type,
                Alias = alias,
                Properties = new Dictionary<string, Expression> { { "id", idExpr } },
              }
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

  // Area declaration
  private static Parser<AreaDeclaration> AreaDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("area")
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
                                new AreaDeclaration
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
  // Format: home [displayName] alias { ... }
  // Examples:
  //   home MyFlat { ... }
  //   home "Моя хата" MyFlat { ... }
  private static Parser<HomeDeclaration> HomeDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("home")
          .Then(_ =>
            StringLiteral
              .Optional()
              .Then(displayNameOpt =>
                Identifier.Then(alias =>
                  Token("{")
                    .Then(_ =>
                      Settings
                        .Optional()
                        .Then(settings =>
                          AreaDeclaration
                            .Many()
                            .Then(areas =>
                              Token("}")
                                .Return(
                                  new HomeDeclaration
                                  {
                                    DisplayName = displayNameOpt.IsDefined
                                      ? displayNameOpt.Get()
                                      : string.Empty,
                                    Alias = alias,
                                    Decorators = decorators.IsDefined
                                      ? decorators.Get()
                                      : new List<Decorator>(),
                                    Settings = settings.IsDefined ? settings.Get() : null,
                                    Areas = areas.ToList(),
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
