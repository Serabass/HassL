using Sprache;
using HassLanguage.Core.Ast;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Program
  public static Parser<Program> Program =>
      SkipWhitespace.Many().Then(_ =>
          HomeDeclaration.Many().Then(homes =>
              AutomationDeclaration.Many().Then(automations =>
                  SkipWhitespace.Many().Then(_ =>
                      Sprache.Parse.Return(new Program
                      {
                        Homes = homes.ToList(),
                        Automations = automations.ToList()
                      })))));

  public static Program Parse(string input)
  {
    var result = Program.Parse(input);
    return result;
  }
}
