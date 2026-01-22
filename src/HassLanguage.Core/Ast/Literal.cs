namespace HassLanguage.Core.Ast;

public abstract class Literal : IAstNode { }

public class BooleanLiteral : Literal
{
  public bool Value { get; set; }
}

public class NumericLiteral : Literal
{
  public object Value { get; set; } = null!; // int or float
  public bool IsFloat { get; set; }
}

public class StringLiteral : Literal
{
  public string Value { get; set; } = string.Empty;
}

public class DurationLiteral : Literal
{
  public Duration Value { get; set; } = null!;
}

public class TimeOfDayLiteral : Literal
{
  public TimeOfDay Value { get; set; } = null!;
}

public class DateTimeLiteral : Literal
{
  public DateTime Value { get; set; }
}

public class ObjectLiteral : Literal
{
  public Dictionary<string, Expression> Properties { get; set; } = new();
}
