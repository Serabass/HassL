using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Recursive parsers
  private static readonly Parser<FunctionCall> FunctionCall = Sprache.Parse.Ref(() =>
    FunctionCallImpl
  );
  private static readonly Parser<Reference> Reference = Sprache.Parse.Ref(() => ReferenceImpl);

  // References
  private static Parser<Reference> ReferenceImpl =>
    Identifier.Then(first =>
      (
        Token(".")
          .Then(_ => Identifier)
          .Many()
          .Select(rest =>
          {
            var parts = new List<string> { first };
            parts.AddRange(rest);
            return new Reference { Parts = parts };
          })
      ).Or(Sprache.Parse.Return(new Reference { Parts = new List<string> { first } }))
    );

  // Function calls
  private static Parser<FunctionCall> FunctionCallWithTarget =>
    Identifier.Then(target =>
      Token(".")
        .Then(_ =>
          Identifier.Then(name =>
            ParseArguments()
              .Select(args => new FunctionCall
              {
                Target = target,
                Name = name,
                Arguments = args,
              })
          )
        )
    );

  private static Parser<FunctionCall> FunctionCallWithoutTarget =>
    Identifier.Then(name =>
      ParseArguments().Select(args => new FunctionCall { Name = name, Arguments = args })
    );

  private static Parser<List<Expression>> ParseArguments() =>
    Token("(")
      .Then(_ =>
        Expression
          .DelimitedBy(Token(","))
          .Contained(SkipWhitespace, SkipWhitespace)
          .Optional()
          .Then(args =>
            Token(")").Return(args.IsDefined ? args.Get().ToList() : new List<Expression>())
          )
      );

  private static Parser<FunctionCall> FunctionCallImpl =>
    FunctionCallWithTarget.Or(FunctionCallWithoutTarget);
}
