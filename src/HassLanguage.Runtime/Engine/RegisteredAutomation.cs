using HassLanguage.Core.Ast;

namespace HassLanguage.Runtime.Engine;

public class RegisteredAutomation
{
  public AutomationDeclaration Declaration { get; set; } = null!;
  public AutomationState State { get; set; }
}

public enum AutomationState
{
  Idle,
  Running,
  Waiting,
  Paused,
}

public class ConditionState
{
  public ConditionExpression Condition { get; set; } = null!;
  public bool IsActive { get; set; }
  public DateTime? StartTime { get; set; }
}
