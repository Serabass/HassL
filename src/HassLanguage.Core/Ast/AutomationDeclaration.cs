namespace HassLanguage.Core.Ast;

public class AutomationDeclaration : IAstNode
{
  public string DisplayName { get; set; } = string.Empty;
  public List<Decorator> Decorators { get; set; } = new();
  public WhenClause WhenClause { get; set; } = null!;
}
