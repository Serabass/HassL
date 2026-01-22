namespace HassLanguage.Core.Ast;

public class Decorator : IAstNode
{
    public string Name { get; set; } = string.Empty;
    public List<DecoratorArgument> Arguments { get; set; } = new();
}

public abstract class DecoratorArgument : IAstNode
{
}

public class IdentifierDecoratorArgument : DecoratorArgument
{
    public string Value { get; set; } = string.Empty;
}

public class StringDecoratorArgument : DecoratorArgument
{
    public string Value { get; set; } = string.Empty;
}

public class DurationDecoratorArgument : DecoratorArgument
{
    public Duration Value { get; set; } = null!;
}

public class IntDecoratorArgument : DecoratorArgument
{
    public int Value { get; set; }
}
