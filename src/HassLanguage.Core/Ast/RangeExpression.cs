namespace HassLanguage.Core.Ast;

public abstract class RangeExpression : IAstNode
{
}

public class TimeRangeExpression : RangeExpression
{
    public TimeOfDay Start { get; set; } = null!;
    public TimeOfDay End { get; set; } = null!;
}

public class ValueRangeExpression : RangeExpression
{
    public Expression Start { get; set; } = null!;
    public Expression End { get; set; } = null!;
}
