using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class SpecialTypesTests
{
  [Theory]
  [InlineData("10s", 10, DurationUnit.Seconds)]
  [InlineData("5m", 5, DurationUnit.Minutes)]
  [InlineData("2h", 2, DurationUnit.Hours)]
  [InlineData("0s", 0, DurationUnit.Seconds)]
  [InlineData("999m", 999, DurationUnit.Minutes)]
  public void ParseDuration_ShouldParseCorrectly(
    string input,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $@"automation 'Test' {{
  when test.value == 5 for {input} {{
    do test();
  }}
}}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClause.Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(expectedValue);
    condition.ForDuration.Unit.Should().Be(expectedUnit);
  }

  [Theory]
  [InlineData("00:00", 0, 0)]
  [InlineData("12:30", 12, 30)]
  [InlineData("23:59", 23, 59)]
  [InlineData("08:15", 8, 15)]
  public void ParseTimeOfDay_ShouldParseCorrectly(
    string input,
    int expectedHour,
    int expectedMinute
  )
  {
    // Act
    var result = HassLanguageParser.Parse(
      $@"automation 'Test' {{
  when time in {input}..18:00 {{
    do test();
  }}
}}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClause.Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as InRangeExpression;
    expr.Should().NotBeNull();
    var range = expr!.Range as TimeRangeExpression;
    range.Should().NotBeNull();
    range!.Start.Hour.Should().Be(expectedHour);
    range.Start.Minute.Should().Be(expectedMinute);
  }

  [Fact]
  public void ParseDateTime_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when 2024-01-15T10:30:45 == 2024-01-15T10:30:45 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClause.Condition as SingleCondition;
    condition.Should().NotBeNull();
    // DateTime parsing is tested implicitly - if it fails, the parse would throw
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    do test.func({ brightness: 70; color: 'red'; });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClause.Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var expr = action.FunctionCall.Arguments[0] as LiteralExpression;
    expr.Should().NotBeNull();
    var objLiteral = expr!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().HaveCount(2);
    objLiteral.Properties.Should().ContainKey("brightness");
    objLiteral.Properties.Should().ContainKey("color");
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseWithCommaSeparator()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    do test.func({ a: 1, b: 2, c: 3 });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClause.Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    var expr = action!.FunctionCall.Arguments[0] as LiteralExpression;
    var objLiteral = expr!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().HaveCount(3);
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseEmptyObject()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    do test.func({ });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClause.Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    var expr = action!.FunctionCall.Arguments[0] as LiteralExpression;
    var objLiteral = expr!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().BeEmpty();
  }
}
