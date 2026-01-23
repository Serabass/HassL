using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public class HassLanguageParser
{
  public static Program Parse(string input)
  {
    try
    {
      return SpracheParser.Parse(input);
    }
    catch (Sprache.ParseException ex)
    {
      throw new ParseException($"Parse error: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Parses a single action statement (e.g., "do test.func();")
  /// </summary>
  public static ActionStatement ParseActionStatement(string input)
  {
    try
    {
      return SpracheParser.ParseActionStatement.Parse(input);
    }
    catch (Sprache.ParseException ex)
    {
      throw new ParseException($"Parse error: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Parses an action block (e.g., "{ do test.func1(); do test.func2(); }")
  /// </summary>
  public static ActionBlock ParseActionBlock(string input)
  {
    try
    {
      return SpracheParser.ParseActionBlock.Parse(input);
    }
    catch (Sprache.ParseException ex)
    {
      throw new ParseException($"Parse error: {ex.Message}", ex);
    }
  }
}

public class ParseException(string message, Exception? innerException = null)
  : Exception(message, innerException) { }
