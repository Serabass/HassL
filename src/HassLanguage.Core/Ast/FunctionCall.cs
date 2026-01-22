namespace HassLanguage.Core.Ast;

public class FunctionCall : IAstNode
{
    public string? Target { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Expression> Arguments { get; set; } = new();
}
