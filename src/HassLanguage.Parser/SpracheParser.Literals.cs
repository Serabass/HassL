using Sprache;
using HassLanguage.Core.Ast;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
    // Recursive parsers
    private static readonly Parser<Literal> Literal = Sprache.Parse.Ref(() => LiteralImpl);
    private static readonly Parser<ObjectLiteral> ObjectLiteral = Sprache.Parse.Ref(() => ObjectLiteralImpl);

    // Literals
    private static Parser<Literal> LiteralImpl =>
        BooleanLiteral.Select(b => new BooleanLiteral { Value = b } as Literal)
        .Or(FloatLiteral.Select(f => new NumericLiteral { Value = f, IsFloat = true } as Literal))
        .Or(IntLiteral.Select(i => new NumericLiteral { Value = i, IsFloat = false } as Literal))
        .Or(StringLiteral.Select(s => new StringLiteral { Value = s } as Literal))
        .Or(Duration.Select(d => new DurationLiteral { Value = d } as Literal))
        .Or(TimeOfDay.Select(t => new TimeOfDayLiteral { Value = t } as Literal))
        .Or(DateTimeLiteral.Select(dt => new DateTimeLiteral { Value = dt } as Literal))
        .Or(ObjectLiteral);

    private static Parser<ObjectLiteral> ObjectLiteralImpl =>
        Sprache.Parse.Char('{').Then(_ =>
            SkipWhitespace.Then(_ =>
                ObjectProperty.DelimitedBy(
                    Sprache.Parse.Char(',').Or(Sprache.Parse.Char(';')).Contained(SkipWhitespace, SkipWhitespace))
                    .Contained(SkipWhitespace, SkipWhitespace)
                    .Then(props =>
                        Sprache.Parse.Char('}').Return(new ObjectLiteral 
                        { 
                            Properties = props.ToDictionary(p => p.Key, p => p.Value) 
                        }))));

    private static Parser<(string Key, Expression Value)> ObjectProperty =>
        Identifier.Then(key =>
            Token(":").Then(_ =>
                Expression.Select(value => (key, value))));
}
