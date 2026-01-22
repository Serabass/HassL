namespace HassLanguage.Core.Ast;

public abstract class ActionStatement : IAstNode
{
}

public class DoAction : ActionStatement
{
    public FunctionCall FunctionCall { get; set; } = null!;
}

public class WaitAction : ActionStatement
{
    public ConditionExpression Condition { get; set; } = null!;
    public Duration ForDuration { get; set; } = null!;
    public Duration? Timeout { get; set; }
}
