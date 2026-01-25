using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Recursive parsers
  private static readonly Parser<ConditionExpression> ConditionExpression = Sprache.Parse.Ref(() =>
    ConditionExpressionImpl
  );
  private static readonly Parser<Duration> ForDuration = Sprache.Parse.Ref(() => ForDurationImpl);

  // Condition item - either Expression or nested ConditionExpression (all { ... } or any { ... })
  // Each item ends with ;
  private static readonly Parser<Expression> ConditionItem = Sprache.Parse.Ref(() =>
    ConditionItemImpl
  );

  private static Parser<Expression> ConditionItemImpl =>
    // Try to parse nested condition first (all { ... } or any { ... })
    ParseAllAnyConditionItem("all")
      .Or(ParseAllAnyConditionItem("any"))
      .Or(Expression.Then(expr => Token(";").Return(expr)));

  // Condition expressions
  private static Parser<ConditionExpression> ConditionExpressionImpl =>
    ParseAllAnyConditionExpression(
        "all",
        (conditions, forDur) => new AllCondition { Conditions = conditions, ForDuration = forDur }
      )
      .Or(
        ParseAllAnyConditionExpression(
            "any",
            (conditions, forDur) =>
              new AnyCondition { Conditions = conditions, ForDuration = forDur }
          )
          .Or(
            ForDuration
              .Optional()
              .Then(forDur =>
                Token("{")
                  .Then(_ =>
                    Expression
                      .Then(expr => Token(";").Return(expr))
                      .Many()
                      .Then(conditions =>
                        Token("}")
                          .Return(
                            new AllCondition
                            {
                              Conditions = conditions.ToList(),
                              ForDuration = forDur.IsDefined ? forDur.Get() : null,
                            }
                          )
                      )
                  )
              )
          )
          .Or(
            Expression.Then(expr =>
              ForDuration
                .Optional()
                .Then(forDurOpt =>
                  Sprache.Parse.Return(
                    new SingleCondition
                    {
                      Expression = expr,
                      ForDuration = forDurOpt.IsDefined ? forDurOpt.Get() : null,
                    }
                  )
                )
            )
          )
      );

  private static Parser<Duration> ForDurationImpl => Token("for").Then(_ => Duration);

  // Helper method to parse all/any condition item (returns FunctionCallExpression)
  private static Parser<Expression> ParseAllAnyConditionItem(string keyword) =>
    Token(keyword)
      .Then(_ =>
        ForDuration
          .Optional()
          .Then(forDur =>
            Token("{")
              .Then(_ =>
                ConditionItem
                  .Many()
                  .Then(conditions =>
                    Token("}")
                      .Then(_ =>
                        Token(";")
                          .Return(
                            new FunctionCallExpression
                            {
                              FunctionCall = new FunctionCall
                              {
                                Name = keyword,
                                Arguments = conditions.ToList(),
                              },
                            }
                          )
                      )
                  )
              )
          )
      );

  // Helper method to parse all/any condition expression (returns AllCondition/AnyCondition)
  private static Parser<ConditionExpression> ParseAllAnyConditionExpression(
    string keyword,
    Func<List<Expression>, Duration?, ConditionExpression> factory
  ) =>
    Token(keyword)
      .Then(_ =>
        ForDuration
          .Optional()
          .Then(forDur =>
            Token("{")
              .Then(_ =>
                ConditionItem
                  .Many()
                  .Then(conditions =>
                    Token("}")
                      .Return(factory(conditions.ToList(), forDur.IsDefined ? forDur.Get() : null))
                  )
              )
          )
      );
}
