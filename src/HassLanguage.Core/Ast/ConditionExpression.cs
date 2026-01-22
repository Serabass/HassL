namespace HassLanguage.Core.Ast;

public abstract class ConditionExpression : IAstNode { }

public class AllCondition : ConditionExpression
{
  public List<Expression> Conditions { get; set; } = new();
  public Duration? ForDuration { get; set; }
}

public class AnyCondition : ConditionExpression
{
  public List<Expression> Conditions { get; set; } = new();
  public Duration? ForDuration { get; set; }
}

public class SingleCondition : ConditionExpression
{
  public Expression Expression { get; set; } = null!;
  public Duration? ForDuration { get; set; }
}
