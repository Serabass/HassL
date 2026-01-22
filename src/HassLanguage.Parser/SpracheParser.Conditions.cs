using Sprache;
using HassLanguage.Core.Ast;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
    // Recursive parsers
    private static readonly Parser<ConditionExpression> ConditionExpression = Sprache.Parse.Ref(() => ConditionExpressionImpl);
    private static readonly Parser<Duration> ForDuration = Sprache.Parse.Ref(() => ForDurationImpl);

    // Condition expressions
    private static Parser<ConditionExpression> ConditionExpressionImpl =>
        Token("all").Then(_ =>
            ForDuration.Optional().Then(forDur =>
                Token("{").Then(_ =>
                    Expression.DelimitedBy(Token(";"))
                        .Contained(SkipWhitespace, SkipWhitespace)
                        .Then(conditions =>
                            Token("}").Return(new AllCondition 
                            { 
                                Conditions = conditions.ToList(), 
                                ForDuration = forDur.IsDefined ? forDur.Get() : null 
                            } as ConditionExpression)))))
        .Or(Token("any").Then(_ =>
            ForDuration.Optional().Then(forDur =>
                Token("{").Then(_ =>
                    Expression.DelimitedBy(Token(";"))
                        .Contained(SkipWhitespace, SkipWhitespace)
                        .Then(conditions =>
                            Token("}").Return(new AnyCondition 
                            { 
                                Conditions = conditions.ToList(), 
                                ForDuration = forDur.IsDefined ? forDur.Get() : null 
                            } as ConditionExpression)))))
        .Or(ForDuration.Optional().Then(forDur =>
            Token("{").Then(_ =>
                Expression.DelimitedBy(Token(";"))
                    .Contained(SkipWhitespace, SkipWhitespace)
                    .Then(conditions =>
                        Token("}").Return(new AllCondition 
                        { 
                            Conditions = conditions.ToList(), 
                            ForDuration = forDur.IsDefined ? forDur.Get() : null 
                        } as ConditionExpression)))))
        .Or(Expression.Then(expr =>
            ForDuration.Optional().Then(forDurOpt =>
                Sprache.Parse.Return(new SingleCondition 
                { 
                    Expression = expr, 
                    ForDuration = forDurOpt.IsDefined ? forDurOpt.Get() : null 
                } as ConditionExpression)))));

    private static Parser<Duration> ForDurationImpl =>
        Token("for").Then(_ => Duration);

    private static Parser<ConditionExpression> ParseSingleCondition() =>
        Expression.Then(expr =>
            ForDuration.Optional().Then(forDurOpt =>
                Sprache.Parse.Return(new SingleCondition 
                { 
                    Expression = expr, 
                    ForDuration = forDurOpt.IsDefined ? forDurOpt.Get() : null 
                } as ConditionExpression)));
}
