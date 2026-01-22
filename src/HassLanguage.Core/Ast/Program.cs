namespace HassLanguage.Core.Ast;

public class Program : IAstNode
{
  public List<HomeDeclaration> Homes { get; set; } = new();
  public List<AutomationDeclaration> Automations { get; set; } = new();
}
