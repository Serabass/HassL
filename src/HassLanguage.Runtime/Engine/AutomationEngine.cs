using HassLanguage.Core.Ast;
using HassLanguage.Core.Validation;

namespace HassLanguage.Runtime.Engine;

public class AutomationEngine
{
  private readonly List<RegisteredAutomation> _automations = new();
  private readonly Dictionary<string, ConditionState> _conditionStates = new();

  public void RegisterAutomation(AutomationDeclaration automation)
  {
    var registered = new RegisteredAutomation
    {
      Declaration = automation,
      State = AutomationState.Idle,
    };
    _automations.Add(registered);
  }

  public void ProcessEvent(object eventData)
  {
    // TODO: Normalize event to StateChangedEvent, CommandEvent, or ScheduleEvent
    // For now, just process all automations

    foreach (var automation in _automations)
    {
      ProcessAutomation(automation, eventData);
    }
  }

  private void ProcessAutomation(RegisteredAutomation automation, object eventData)
  {
    foreach (var when in automation.Declaration.WhenClauses)
    {
      ProcessWhenClause(automation, when, eventData);
    }
  }

  private void ProcessWhenClause(RegisteredAutomation automation, WhenClause when, object eventData)
  {
    var conditionKey = GetConditionKey(automation, when);

    if (!_conditionStates.TryGetValue(conditionKey, out var state))
    {
      state = new ConditionState
      {
        Condition = when.Condition,
        IsActive = false,
        StartTime = null,
      };
      _conditionStates[conditionKey] = state;
    }

    var isSatisfied = EvaluateCondition(when.Condition, eventData);

    if (isSatisfied)
    {
      if (!state.IsActive)
      {
        state.IsActive = true;
        state.StartTime = DateTime.UtcNow;
      }

      // Check if condition held for required duration
      if (when.Condition is SingleCondition single && single.ForDuration != null)
      {
        if (state.StartTime.HasValue)
        {
          var elapsed = DateTime.UtcNow - state.StartTime.Value;
          if (elapsed >= single.ForDuration.ToTimeSpan())
          {
            // Condition held long enough, trigger action
            ExecuteActions(when.Actions);
            state.IsActive = false;
            state.StartTime = null;
          }
        }
      }
      else if (when.Condition is AllCondition all && all.ForDuration != null)
      {
        if (state.StartTime.HasValue)
        {
          var elapsed = DateTime.UtcNow - state.StartTime.Value;
          if (elapsed >= all.ForDuration.ToTimeSpan())
          {
            ExecuteActions(when.Actions);
            state.IsActive = false;
            state.StartTime = null;
          }
        }
      }
      else if (when.Condition is AnyCondition any && any.ForDuration != null)
      {
        if (state.StartTime.HasValue)
        {
          var elapsed = DateTime.UtcNow - state.StartTime.Value;
          if (elapsed >= any.ForDuration.ToTimeSpan())
          {
            ExecuteActions(when.Actions);
            state.IsActive = false;
            state.StartTime = null;
          }
        }
      }
      else
      {
        // No duration requirement, trigger immediately
        ExecuteActions(when.Actions);
      }
    }
    else
    {
      // Condition not satisfied, reset state
      if (state.IsActive)
      {
        state.IsActive = false;
        state.StartTime = null;
      }
    }
  }

  private bool EvaluateCondition(ConditionExpression condition, object eventData)
  {
    return condition switch
    {
      SingleCondition single => EvaluateExpression(single.Expression, eventData),
      AllCondition all => all.Conditions.All(c => EvaluateExpression(c, eventData)),
      AnyCondition any => any.Conditions.Any(c => EvaluateExpression(c, eventData)),
      _ => false,
    };
  }

  private bool EvaluateExpression(Expression expression, object eventData)
  {
    // TODO: Implement proper expression evaluation
    // For now, return false as placeholder
    return false;
  }

  private void ExecuteActions(ActionBlock actions)
  {
    foreach (var action in actions.Statements)
    {
      ExecuteAction(action);
    }
  }

  private void ExecuteAction(ActionStatement action)
  {
    switch (action)
    {
      case CallAction callAction:
        ExecuteFunctionCall(callAction.FunctionCall);
        break;
      case WaitAction waitAction:
        // TODO: Register wait condition (non-blocking)
        RegisterWaitCondition(waitAction);
        break;
    }
  }

  private void ExecuteFunctionCall(FunctionCall call)
  {
    // TODO: Implement function call execution
    // This will connect to Home Assistant API, HTTP clients, etc.
  }

  private void RegisterWaitCondition(WaitAction waitAction)
  {
    // TODO: Register wait condition with timeout
    // This should be non-blocking and use state machine
  }

  private string GetConditionKey(RegisteredAutomation automation, WhenClause when)
  {
    return $"{automation.Declaration.DisplayName}_{when.GetHashCode()}";
  }
}
