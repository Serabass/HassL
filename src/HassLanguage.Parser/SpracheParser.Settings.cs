using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Settings parsers - simplified to minimum
  private static Parser<Settings> Settings =>
    Token("settings").Then(_ => Token("{").Then(_ => Token("}").Return(new Settings())));
}
