using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Recursive parsers - must be declared as fields
  private static readonly Parser<Expression> Expression = Sprache.Parse.Ref(() =>
    InRangeExpression
  );
  private static readonly Parser<RangeExpression> RangeExpression = Sprache.Parse.Ref(() =>
    RangeExpressionImpl
  );

  // Expression hierarchy
  private static Parser<Expression> PrimaryExpression =>
    Expression
      .Contained(Sprache.Parse.Char('('), Sprache.Parse.Char(')'))
      .Select(e => new ParenExpression { Inner = e } as Expression)
      .Or(FunctionCall.Select(f => new FunctionCallExpression { FunctionCall = f } as Expression))
      .Or(Literal.Select(l => new LiteralExpression { Literal = l } as Expression))
      .Or(Reference.Select(r => new ReferenceExpression { Reference = r } as Expression));

  private static Parser<Expression> ComparisonExpression =>
    PrimaryExpression.Then(left =>
      (
        Token("==")
          .Return(BinaryOperator.Equals)
          .Or(Token("!=").Return(BinaryOperator.NotEquals))
          .Or(Token(">=").Return(BinaryOperator.GreaterOrEqual))
          .Or(Token("<=").Return(BinaryOperator.LessOrEqual))
          .Or(Token(">").Return(BinaryOperator.Greater))
          .Or(Token("<").Return(BinaryOperator.Less))
          .Then(op =>
            PrimaryExpression.Select(right =>
              new BinaryExpression
              {
                Left = left,
                Right = right,
                Operator = op,
              } as Expression
            )
          )
      ).Or(Sprache.Parse.Return(left))
    );

  private static Parser<Expression> AndExpression =>
    ComparisonExpression.Then(left =>
      (
        Token("&&")
          .Then(_ =>
            ComparisonExpression.Select(right =>
              new BinaryExpression
              {
                Left = left,
                Right = right,
                Operator = BinaryOperator.And,
              } as Expression
            )
          )
      ).Or(Sprache.Parse.Return(left))
    );

  private static Parser<Expression> OrExpression =>
    AndExpression.Then(left =>
      (
        Token("||")
          .Then(_ =>
            AndExpression.Select(right =>
              new BinaryExpression
              {
                Left = left,
                Right = right,
                Operator = BinaryOperator.Or,
              } as Expression
            )
          )
      ).Or(Sprache.Parse.Return(left))
    );

  private static Parser<Expression> InRangeExpression =>
    OrExpression.Then(left =>
      Token("in")
        .Then(_ =>
          RangeExpression.Select(range =>
            new InRangeExpression { Left = left, Range = range } as Expression
          )
        )
        .Or(Sprache.Parse.Return(left))
    );

  // Range expressions
  private static Parser<RangeExpression> RangeExpressionImpl =>
    TimeOfDay
      .Then(start =>
        Token("..")
          .Then(_ =>
            TimeOfDay.Select(end =>
              new TimeRangeExpression { Start = start, End = end } as RangeExpression
            )
          )
      )
      .Or(
        PrimaryExpression.Then(start =>
          Token("..")
            .Then(_ =>
              PrimaryExpression.Select(end =>
                new ValueRangeExpression { Start = start, End = end } as RangeExpression
              )
            )
        )
      );

  // Public parser for testing - parse expression from string
  public static Expression ParseExpression(string input)
  {
    return Expression.Parse(input);
  }

  // Public parser for testing - parse range expression from string
  public static RangeExpression ParseRangeExpression(string input)
  {
    return RangeExpression.Parse(input);
  }
}
