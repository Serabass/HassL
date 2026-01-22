namespace HassLanguage.Core.Ast;

public abstract class Expression : IAstNode
{
}

public class InRangeExpression : Expression
{
    public Expression Left { get; set; } = null!;
    public RangeExpression Range { get; set; } = null!;
}

public class BinaryExpression : Expression
{
    public Expression Left { get; set; } = null!;
    public Expression Right { get; set; } = null!;
    public BinaryOperator Operator { get; set; }
}

public enum BinaryOperator
{
    Equals,
    NotEquals,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual,
    And,
    Or
}

public class ParenExpression : Expression
{
    public Expression Inner { get; set; } = null!;
}

public class FunctionCallExpression : Expression
{
    public FunctionCall FunctionCall { get; set; } = null!;
}

public class ReferenceExpression : Expression
{
    public Reference Reference { get; set; } = null!;
}

public class LiteralExpression : Expression
{
    public Literal Literal { get; set; } = null!;
}
