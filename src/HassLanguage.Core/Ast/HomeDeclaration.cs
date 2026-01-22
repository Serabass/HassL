namespace HassLanguage.Core.Ast;

public class HomeDeclaration : IAstNode
{
    public string DisplayName { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public List<Decorator> Decorators { get; set; } = new();
    public Settings? Settings { get; set; }
    public List<RoomDeclaration> Rooms { get; set; } = new();
}
