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
    (
      Token("all")
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
                                  Name = "all",
                                  Arguments = conditions.ToList(),
                                },
                              } as Expression
                            )
                        )
                    )
                )
            )
        )
        .Or(
          Token("any")
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
                                      Name = "any",
                                      Arguments = conditions.ToList(),
                                    },
                                  } as Expression
                                )
                            )
                        )
                    )
                )
            )
        )
        .Or(Expression.Then(expr => Token(";").Return(expr)))
    );

  // Condition expressions
  private static Parser<ConditionExpression> ConditionExpressionImpl =>
    Token("all")
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
                      .Return(
                        new AllCondition
                        {
                          Conditions = conditions.ToList(),
                          ForDuration = forDur.IsDefined ? forDur.Get() : null,
                        } as ConditionExpression
                      )
                  )
              )
          )
      )
      .Or(
        Token("any")
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
                          .Return(
                            new AnyCondition
                            {
                              Conditions = conditions.ToList(),
                              ForDuration = forDur.IsDefined ? forDur.Get() : null,
                            } as ConditionExpression
                          )
                      )
                  )
              )
          )
          .Or(
            ForDuration
              .Optional()
              .Then(forDur =>
                Token("{")
                  .Then(_ =>
                    (Expression.Then(expr => Token(";").Return(expr)).Many()).Then(conditions =>
                      Token("}")
                        .Return(
                          new AllCondition
                          {
                            Conditions = conditions.ToList(),
                            ForDuration = forDur.IsDefined ? forDur.Get() : null,
                          } as ConditionExpression
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
                    } as ConditionExpression
                  )
                )
            )
          )
      );

  private static Parser<Duration> ForDurationImpl => Token("for").Then(_ => Duration);

  private static Parser<ConditionExpression> ParseSingleCondition() =>
    Expression.Then(expr =>
      ForDuration
        .Optional()
        .Then(forDurOpt =>
          Sprache.Parse.Return(
            new SingleCondition
            {
              Expression = expr,
              ForDuration = forDurOpt.IsDefined ? forDurOpt.Get() : null,
            } as ConditionExpression
          )
        )
    );
}
