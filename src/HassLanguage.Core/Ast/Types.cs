namespace HassLanguage.Core.Ast;

public class Duration
{
  public int Value { get; set; }
  public DurationUnit Unit { get; set; }

  public TimeSpan ToTimeSpan()
  {
    return Unit switch
    {
      DurationUnit.Seconds => TimeSpan.FromSeconds(Value),
      DurationUnit.Minutes => TimeSpan.FromMinutes(Value),
      DurationUnit.Hours => TimeSpan.FromHours(Value),
      _ => throw new ArgumentException($"Unknown duration unit: {Unit}"),
    };
  }
}

public enum DurationUnit
{
  Seconds,
  Minutes,
  Hours,
}

public class TimeOfDay
{
  public int Hour { get; set; }
  public int Minute { get; set; }

  public TimeSpan ToTimeSpan()
  {
    return new TimeSpan(Hour, Minute, 0);
  }

  public bool IsInRange(TimeOfDay start, TimeOfDay end)
  {
    var thisTime = ToTimeSpan();
    var startTime = start.ToTimeSpan();
    var endTime = end.ToTimeSpan();

    // Handle range crossing midnight (e.g., 23:00..06:00)
    if (startTime > endTime)
    {
      return thisTime >= startTime || thisTime <= endTime;
    }

    return thisTime >= startTime && thisTime <= endTime;
  }
}
