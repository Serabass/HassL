namespace HassLanguage.Core.Ast;

public class WhenClause : IAstNode
{
  public List<Decorator> Decorators { get; set; } = new();
  public ConditionExpression Condition { get; set; } = null!;
  public ActionBlock Actions { get; set; } = null!;
}
