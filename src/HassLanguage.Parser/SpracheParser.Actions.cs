using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Action statements
  private static Parser<ActionStatement> ActionStatement =>
    Token("do")
      .Then(_ =>
        FunctionCall.Then(call =>
          Token(";").Return(new DoAction { FunctionCall = call } as ActionStatement)
        )
      )
      .Or(ParseWaitAction());

  private static Parser<ActionStatement> ParseWaitAction() =>
    Token("wait")
      .Then(_ =>
        ConditionExpression.Then(condition =>
          Token("for")
            .Then(_ =>
              Duration.Then(forDur =>
                (Token("timeout").Then(_ => Duration).Optional()).Then(timeout =>
                  Token(";")
                    .Return(
                      new WaitAction
                      {
                        Condition = condition,
                        ForDuration = forDur,
                        Timeout = timeout.IsDefined ? timeout.Get() : null,
                      } as ActionStatement
                    )
                )
              )
            )
        )
      );

  private static Parser<ActionBlock> ActionBlock =>
    Token("{")
      .Then(_ =>
        ActionStatement
          .Many()
          .Then(statements =>
            Token("}").Return(new ActionBlock { Statements = statements.ToList() })
          )
      );

  // When clause
  private static Parser<WhenClause> WhenClause =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("when")
          .Then(_ =>
            ConditionExpression.Then(condition =>
              ActionBlock.Then(actions =>
                Sprache.Parse.Return(
                  new WhenClause
                  {
                    Decorators = decorators.IsDefined ? decorators.Get() : new List<Decorator>(),
                    Condition = condition,
                    Actions = actions,
                  }
                )
              )
            )
          )
      );

  // Automation declaration
  private static Parser<AutomationDeclaration> AutomationDeclaration =>
    DecoratorList
      .Optional()
      .Then(decorators =>
        Token("automation")
          .Then(_ =>
            StringLiteral.Then(displayName =>
              Token("{")
                .Then(_ =>
                  WhenClause
                    .Many()
                    .Then(whenClauses =>
                      Token("}")
                        .Return(
                          new AutomationDeclaration
                          {
                            DisplayName = displayName,
                            Decorators = decorators.IsDefined
                              ? decorators.Get()
                              : new List<Decorator>(),
                            WhenClauses = whenClauses.ToList(),
                          }
                        )
                    )
                )
            )
          )
      );
}
