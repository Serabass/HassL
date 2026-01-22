using HassLanguage.Core.Ast;

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
}

public class ParseException(string message, Exception? innerException = null)
  : Exception(message, innerException) { }
