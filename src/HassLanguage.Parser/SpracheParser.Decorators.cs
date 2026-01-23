using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Decorators
  private static Parser<Decorator> Decorator =>
    Token(
      Sprache
        .Parse.Char('@')
        .Then(_ =>
          Identifier.Then(name =>
            Sprache
              .Parse.Char('(')
              .Then(_ =>
                DecoratorArgument
                  .DelimitedBy(Sprache.Parse.Char(',').Contained(SkipWhitespace, SkipWhitespace))
                  .Contained(SkipWhitespace, SkipWhitespace)
                  .Then(args =>
                    Sprache
                      .Parse.Char(')')
                      .Return(new Decorator { Name = name, Arguments = args.ToList() })
                  )
              )
          )
        )
    );

  private static Parser<DecoratorArgument> DecoratorArgument =>
    Identifier
      .Select(id => new IdentifierDecoratorArgument { Value = id } as DecoratorArgument)
      .Or(StringLiteral.Select(s => new StringDecoratorArgument { Value = s } as DecoratorArgument))
      .Or(Duration.Select(d => new DurationDecoratorArgument { Value = d } as DecoratorArgument))
      .Or(IntLiteral.Select(i => new IntDecoratorArgument { Value = i } as DecoratorArgument));

  private static Parser<List<Decorator>> DecoratorList =>
    Decorator.AtLeastOnce().Select(d => d.ToList());
}
