using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
    // Whitespace and comments
    private static readonly Parser<char> WhitespaceChar = Sprache.Parse.WhiteSpace;
    private static readonly Parser<string> LineComment = 
        Sprache.Parse.String("//").Then(_ => Sprache.Parse.AnyChar.Except(Sprache.Parse.LineEnd).Many().Text());
    private static readonly Parser<string> BlockComment = 
        Sprache.Parse.String("/*").Then(_ => Sprache.Parse.AnyChar.Until(Sprache.Parse.String("*/")).Text());
    private static readonly Parser<object?> SkipWhitespace = 
        Sprache.Parse.WhiteSpace.Select(_ => (object?)null).Or(LineComment.Select(_ => (object?)null)).Or(BlockComment.Select(_ => (object?)null)).Many().Select(_ => (object?)null);

    // Basic tokens
    private static Parser<T> Token<T>(Parser<T> parser) => parser.Contained(SkipWhitespace, SkipWhitespace);
    private static Parser<string> Token(string text) => Token(Sprache.Parse.String(text).Text());

    // Identifiers and literals
    private static readonly Parser<string> Identifier = Token(
        Sprache.Parse.Letter.Or(Sprache.Parse.Char('_')).Then(c => 
            Sprache.Parse.LetterOrDigit.Or(Sprache.Parse.Char('_')).Many().Text()
            .Select(s => c.ToString() + s))
    );

    private static readonly Parser<string> StringLiteral = Token(
        Sprache.Parse.Char('"').Then(_ => 
            Sprache.Parse.CharExcept('"').Many().Text())
            .Contained(Sprache.Parse.Char('"'), Sprache.Parse.Char('"'))
    );

    private static readonly Parser<int> IntLiteral = Token(Sprache.Parse.Number.Select(int.Parse));
    private static readonly Parser<double> FloatLiteral = Token(
        Sprache.Parse.Number.Then(n => Sprache.Parse.Char('.').Then(_ => Sprache.Parse.Number).Select(d => n + "." + d))
            .Select(double.Parse)
    );

    private static readonly Parser<bool> BooleanLiteral = Token(
        Sprache.Parse.String("true").Return(true).Or(Sprache.Parse.String("false").Return(false))
    );
}
