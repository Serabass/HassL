namespace HassLanguage.Core.Ast;

public class Program : IAstNode
{
  public List<ZoneDeclaration> Zones { get; set; } = new();
  public List<AutomationDeclaration> Automations { get; set; } = new();
}
