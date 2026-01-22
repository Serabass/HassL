namespace HassLanguage.Core.Ast;

public class EntityDeclaration : IAstNode
{
    public string Type { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public Dictionary<string, Expression> Properties { get; set; } = new();
}
