using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class ConditionsTests
{
  [Fact]
  public void ParseSingleCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].WhenClauses.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.Expression.Should().NotBeNull();
  }

  [Fact]
  public void ParseSingleConditionWithForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 for 30m { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(30);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseAllCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when all { test.value > 5; test.value < 10; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
  }

  [Fact]
  public void ParseAllConditionWithForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when all for 5m { test.value > 5; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(5);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseAnyCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when any { test.value > 5; test.value < 0; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
  }

  [Fact]
  public void ParseAnyConditionWithForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when any for 10s { test.value > 5; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(10);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseImplicitAllCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when for 1h { test.value > 5; test.value < 10; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(1);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Hours);
  }

  [Theory]
  [InlineData("5s", 5, DurationUnit.Seconds)]
  [InlineData("30m", 30, DurationUnit.Minutes)]
  [InlineData("2h", 2, DurationUnit.Hours)]
  [InlineData("0s", 0, DurationUnit.Seconds)]
  [InlineData("999m", 999, DurationUnit.Minutes)]
  public void ParseSingleConditionWithForDuration_ShouldParseAllUnits(
    string duration,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when test.value == 5 for {duration} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(expectedValue);
    condition.ForDuration.Unit.Should().Be(expectedUnit);
  }

  [Theory]
  [InlineData("15s", 15, DurationUnit.Seconds)]
  [InlineData("45m", 45, DurationUnit.Minutes)]
  [InlineData("3h", 3, DurationUnit.Hours)]
  [InlineData("1s", 1, DurationUnit.Seconds)]
  public void ParseAllConditionWithForDuration_ShouldParseAllUnits(
    string duration,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when all for {duration} {{ test.value > 5; }} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(expectedValue);
    condition.ForDuration.Unit.Should().Be(expectedUnit);
    condition.Conditions.Should().HaveCount(1);
  }

  [Theory]
  [InlineData("20s", 20, DurationUnit.Seconds)]
  [InlineData("60m", 60, DurationUnit.Minutes)]
  [InlineData("24h", 24, DurationUnit.Hours)]
  public void ParseAnyConditionWithForDuration_ShouldParseAllUnits(
    string duration,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when any for {duration} {{ test.value > 5; }} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(expectedValue);
    condition.ForDuration.Unit.Should().Be(expectedUnit);
    condition.Conditions.Should().HaveCount(1);
  }

  [Theory]
  [InlineData("10s", 10, DurationUnit.Seconds)]
  [InlineData("30m", 30, DurationUnit.Minutes)]
  [InlineData("12h", 12, DurationUnit.Hours)]
  public void ParseImplicitAllConditionWithForDuration_ShouldParseAllUnits(
    string duration,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when for {duration} {{ test.value > 5; }} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(expectedValue);
    condition.ForDuration.Unit.Should().Be(expectedUnit);
    condition.Conditions.Should().HaveCount(1);
  }

  [Fact]
  public void ParseSingleConditionWithForDuration_ShouldPreserveExpression()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when sensor.temperature > 25 for 5m { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.Expression.Should().NotBeNull();
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(5);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseAllConditionWithForDuration_ShouldPreserveMultipleConditions()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when all for 10s { temp > 20; humidity < 80; pressure > 1000; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(3);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(10);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseAnyConditionWithForDuration_ShouldPreserveMultipleConditions()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when any for 1h { door.open == true; window.open == true; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(1);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Hours);
  }

  [Fact]
  public void ParseSingleConditionWithoutForDuration_ShouldNotHaveForDuration()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().BeNull();
  }

  [Fact]
  public void ParseAllConditionWithoutForDuration_ShouldNotHaveForDuration()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when all { test.value > 5; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().BeNull();
  }

  [Fact]
  public void ParseAnyConditionWithoutForDuration_ShouldNotHaveForDuration()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when any { test.value > 5; } { do test(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().BeNull();
  }
}
