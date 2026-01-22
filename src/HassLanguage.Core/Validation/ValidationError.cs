using HassLanguage.Core.Ast;

namespace HassLanguage.Core.Validation;

public class ValidationError
{
  public string Message { get; }
  public IAstNode? Node { get; }

  public ValidationError(string message, IAstNode? node = null)
  {
    Message = message;
    Node = node;
  }

  public override string ToString() => Message;
}
