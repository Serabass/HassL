namespace HassLanguage.Core.Ast;

public class ActionBlock : IAstNode
{
  public List<ActionStatement> Statements { get; set; } = new();
}
