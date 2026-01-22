using HassLanguage.Core.Ast;
using HassLanguage.Core.Validation;
using HassLanguage.Parser;
using HassLanguage.Runtime.Engine;

namespace HassLanguage.Runtime;

public class HassLanguageRuntime
{
  private readonly AutomationEngine _engine;
  private readonly SemanticValidator _validator;

  public HassLanguageRuntime()
  {
    _engine = new AutomationEngine();
    _validator = new SemanticValidator();
  }

  public void LoadProgram(string sourceCode)
  {
    var program = HassLanguageParser.Parse(sourceCode);
    var validationResult = _validator.Validate(program);

    if (!validationResult.IsValid)
    {
      var errors = string.Join("\n", validationResult.Errors.Select(e => e.Message));
      throw new InvalidOperationException($"Validation failed:\n{errors}");
    }

    foreach (var automation in program.Automations)
    {
      _engine.RegisterAutomation(automation);
    }
  }

  public void ProcessEvent(object eventData)
  {
    _engine.ProcessEvent(eventData);
  }
}
