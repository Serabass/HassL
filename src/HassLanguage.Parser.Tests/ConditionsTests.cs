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
}
