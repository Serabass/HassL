using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Program
  public static Parser<Program> Program =>
    SkipWhitespace
      .Many()
      .Then(_ =>
        ZoneDeclaration
          .Many()
          .Then(zones =>
            AutomationDeclaration
              .Many()
              .Then(automations =>
                SkipWhitespace
                  .Many()
                  .Then(_ =>
                    Sprache.Parse.Return(
                      new Program { Zones = zones.ToList(), Automations = automations.ToList() }
                    )
                  )
              )
          )
      );

  public static Program Parse(string input)
  {
    var result = Program.Parse(input);
    return result;
  }
}
