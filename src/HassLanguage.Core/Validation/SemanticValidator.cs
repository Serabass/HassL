using HassLanguage.Core.Ast;

namespace HassLanguage.Core.Validation;

public class SemanticValidator
{
  private readonly List<ValidationError> _errors = new();
  private readonly SymbolTable _symbols = new();

  public ValidationResult Validate(Program program)
  {
    _errors.Clear();
    _symbols.Clear();

    // Build symbol table
    foreach (var home in program.Homes)
    {
      ValidateHome(home);
    }

    // Validate automations
    foreach (var automation in program.Automations)
    {
      ValidateAutomation(automation);
    }

    return new ValidationResult { IsValid = _errors.Count == 0, Errors = _errors };
  }

  private void ValidateHome(HomeDeclaration home)
  {
    // Check home alias uniqueness
    if (!_symbols.AddHome(home.Alias, home))
    {
      _errors.Add(new ValidationError($"Home alias '{home.Alias}' is already defined", home));
      return;
    }

    foreach (var area in home.Areas)
    {
      ValidateArea(home.Alias, area);
    }
  }

  private void ValidateArea(string homeAlias, AreaDeclaration area)
  {
    var fullPath = $"{homeAlias}.{area.Alias}";

    if (!_symbols.AddArea(fullPath, area))
    {
      _errors.Add(
        new ValidationError(
          $"Area alias '{area.Alias}' is already defined in home '{homeAlias}'",
          area
        )
      );
      return;
    }

    foreach (var device in area.Devices)
    {
      ValidateDevice(fullPath, device);
    }
  }

  private void ValidateDevice(string areaPath, DeviceDeclaration device)
  {
    var fullPath = $"{areaPath}.{device.Alias}";

    if (!_symbols.AddDevice(fullPath, device))
    {
      _errors.Add(
        new ValidationError(
          $"Device alias '{device.Alias}' is already defined in area '{areaPath}'",
          device
        )
      );
      return;
    }

    foreach (var entity in device.Entities)
    {
      ValidateEntity(fullPath, entity);
    }
  }

  private void ValidateEntity(string devicePath, EntityDeclaration entity)
  {
    var fullPath = $"{devicePath}.{entity.Alias}";

    if (!_symbols.AddEntity(fullPath, entity))
    {
      _errors.Add(
        new ValidationError(
          $"Entity alias '{entity.Alias}' is already defined in device '{devicePath}'",
          entity
        )
      );
    }
  }

  private void ValidateAutomation(AutomationDeclaration automation)
  {
    foreach (var when in automation.WhenClauses)
    {
      ValidateWhenClause(when);
    }
  }

  private void ValidateWhenClause(WhenClause when)
  {
    ValidateCondition(when.Condition);

    foreach (var action in when.Actions.Statements)
    {
      ValidateAction(action);
    }
  }

  private void ValidateCondition(ConditionExpression condition)
  {
    switch (condition)
    {
      case AllCondition all:
        foreach (var expr in all.Conditions)
        {
          ValidateExpression(expr);
        }
        break;
      case AnyCondition any:
        foreach (var expr in any.Conditions)
        {
          ValidateExpression(expr);
        }
        break;
      case SingleCondition single:
        ValidateExpression(single.Expression);
        break;
    }
  }

  private void ValidateExpression(Expression expression)
  {
    switch (expression)
    {
      case ReferenceExpression refExpr:
        ValidateReference(refExpr.Reference);
        break;
      case BinaryExpression binExpr:
        ValidateExpression(binExpr.Left);
        ValidateExpression(binExpr.Right);
        // TODO: Type checking for binary operations
        break;
      case InRangeExpression inRange:
        ValidateExpression(inRange.Left);
        // TODO: Type checking for range operations
        break;
      case FunctionCallExpression funcExpr:
        ValidateFunctionCall(funcExpr.FunctionCall);
        break;
      case ParenExpression paren:
        ValidateExpression(paren.Inner);
        break;
      case LiteralExpression:
        // Literals are always valid
        break;
    }
  }

  private void ValidateReference(Reference reference)
  {
    var path = string.Join(".", reference.Parts);

    if (!_symbols.EntityExists(path))
    {
      _errors.Add(new ValidationError($"Reference '{path}' does not exist", reference));
    }
  }

  private void ValidateFunctionCall(FunctionCall call)
  {
    // TODO: Validate function call arguments count and types
    foreach (var arg in call.Arguments)
    {
      ValidateExpression(arg);
    }
  }

  private void ValidateAction(ActionStatement action)
  {
    switch (action)
    {
      case DoAction doAction:
        ValidateFunctionCall(doAction.FunctionCall);
        break;
      case WaitAction waitAction:
        ValidateCondition(waitAction.Condition);
        // TODO: Validate duration and timeout
        break;
    }
  }
}
