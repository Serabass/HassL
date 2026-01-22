namespace HassLanguage.Core.Validation;

public class ValidationResult
{
  public bool IsValid { get; set; }
  public List<ValidationError> Errors { get; set; } = new();
}
