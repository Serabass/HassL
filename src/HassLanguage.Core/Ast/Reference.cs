namespace HassLanguage.Core.Ast;

public class Reference : IAstNode
{
    public List<string> Parts { get; set; } = new();
}
