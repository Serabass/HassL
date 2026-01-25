using HassLanguage.Core.Ast;
using Sprache;

namespace HassLanguage.Parser;

public static partial class SpracheParser
{
  // Duration
  private static readonly Parser<DurationUnit> DurationUnitParser = Token(
    Sprache
      .Parse.Char('s')
      .Return(DurationUnit.Seconds)
      .Or(Sprache.Parse.Char('m').Return(DurationUnit.Minutes))
      .Or(Sprache.Parse.Char('h').Return(DurationUnit.Hours))
  );

  private static readonly Parser<Duration> Duration = IntLiteral.Then(v =>
    DurationUnitParser.Select(u => new Duration { Value = v, Unit = u })
  );

  // Time of day
  private static readonly Parser<TimeOfDay> TimeOfDay = Token(
    Sprache
      .Parse.Digit.Repeat(2)
      .Text()
      .Then(h =>
        Sprache
          .Parse.Char(':')
          .Then(_ =>
            Sprache
              .Parse.Digit.Repeat(2)
              .Text()
              .Select(m => new TimeOfDay { Hour = int.Parse(h), Minute = int.Parse(m) })
          )
      )
  );

  // DateTime (simplified)
  private static readonly Parser<DateTime> DateTimeLiteral = Token(
    Sprache
      .Parse.Digit.Repeat(4)
      .Text()
      .Then(y =>
        Sprache
          .Parse.Char('-')
          .Then(_ => Sprache.Parse.Digit.Repeat(2).Text())
          .Then(m =>
            Sprache
              .Parse.Char('-')
              .Then(_ => Sprache.Parse.Digit.Repeat(2).Text())
              .Then(d =>
                Sprache
                  .Parse.Char('T')
                  .Then(_ => Sprache.Parse.Digit.Repeat(2).Text())
                  .Then(h =>
                    Sprache
                      .Parse.Char(':')
                      .Then(_ => Sprache.Parse.Digit.Repeat(2).Text())
                      .Then(min =>
                        Sprache
                          .Parse.Char(':')
                          .Then(_ => Sprache.Parse.Digit.Repeat(2).Text())
                          .Select(s =>
                            DateTime.Parse(y + "-" + m + "-" + d + "T" + h + ":" + min + ":" + s)
                          )
                      )
                  )
              )
          )
      )
  );
}
