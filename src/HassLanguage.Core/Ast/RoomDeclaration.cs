namespace HassLanguage.Core.Ast;

public class RoomDeclaration : IAstNode
{
  public string DisplayName { get; set; } = string.Empty;
  public string Alias { get; set; } = string.Empty;
  public string? Type { get; set; }
  public List<Decorator> Decorators { get; set; } = new();
  public List<DeviceDeclaration> Devices { get; set; } = new();
}
