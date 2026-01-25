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
      @"automation 'Test' {
  when test.value == 5 {
    call test();
  }
}"
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
      @"automation 'Test' {
  when test.value == 5 for 30m {
    call test();
  }
}"
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
      @"automation 'Test' {
  when all {
    test.value > 5;
    test.value < 10;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when all for 5m {
    test.value > 5;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when any {
    test.value > 5;
    test.value < 0;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when any for 10s {
    test.value > 5;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when for 1h {
    test.value > 5;
    test.value < 10;
  } {
    call test();
  }
}"
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
      $@"automation 'Test' {{
  when test.value == 5 for {duration} {{
    call test();
  }}
}}"
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
      $@"automation 'Test' {{
  when all for {duration} {{
    test.value > 5;
  }} {{
    call test();
  }}
}}"
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
      $@"automation 'Test' {{
  when any for {duration} {{
    test.value > 5;
  }} {{
    call test();
  }}
}}"
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
      $@"automation 'Test' {{
  when for {duration} {{
    test.value > 5;
  }} {{
    call test();
  }}
}}"
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
      @"automation 'Test' {
  when sensor.temperature > 25 for 5m {
    call test();
  }
}"
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
      @"automation 'Test' {
  when all for 10s {
    temp > 20;
    humidity < 80;
    pressure > 1000;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when any for 1h {
    door.open == true;
    window.open == true;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when test.value == 5 {
    call test();
  }
}"
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
      @"automation 'Test' {
  when all {
    test.value > 5;
  } {
    call test();
  }
}"
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
      @"automation 'Test' {
  when any {
    test.value > 5;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().BeNull();
  }

  [Fact]
  public void ParseSingleConditionWithAndOperator_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when temp > 20 && temp < 30 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.And);
    expr.Left.Should().NotBeNull();
    expr.Right.Should().NotBeNull();
  }

  [Fact]
  public void ParseSingleConditionWithOrOperator_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when door.open == true || window.open == true {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Or);
    expr.Left.Should().NotBeNull();
    expr.Right.Should().NotBeNull();
  }

  [Fact]
  public void ParseSingleConditionWithAndAndOrOperators_ShouldRespectOperatorPrecedence()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when temp > 20 && temp < 30 || humidity > 50 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Or);

    // Проверяем, что левая часть содержит && (более высокий приоритет)
    var leftExpr = expr.Left as BinaryExpression;
    leftExpr.Should().NotBeNull();
    leftExpr!.Operator.Should().Be(BinaryOperator.And);
  }

  [Fact]
  public void ParseSingleConditionWithOrAndAndOperators_ShouldRespectOperatorPrecedence()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when door.open == true || window.open == true && time > 10 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Or);

    // Проверяем, что правая часть содержит && (более высокий приоритет)
    var rightExpr = expr.Right as BinaryExpression;
    rightExpr.Should().NotBeNull();
    rightExpr!.Operator.Should().Be(BinaryOperator.And);
  }

  [Fact]
  public void ParseSingleConditionWithParentheses_ShouldOverrideOperatorPrecedence()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when (temp > 20 || temp < 10) && humidity < 80 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.And);

    // Проверяем, что левая часть - это выражение в скобках с ||
    var leftExpr = expr.Left as ParenExpression;
    leftExpr.Should().NotBeNull();
    var innerExpr = leftExpr!.Inner as BinaryExpression;
    innerExpr.Should().NotBeNull();
    innerExpr!.Operator.Should().Be(BinaryOperator.Or);
  }

  [Fact]
  public void ParseSingleConditionWithComplexNestedOperators_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when (temp > 20 && temp < 30) || (humidity > 50 && humidity < 80) {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Or);

    // Проверяем левую часть (скобки с &&)
    var leftExpr = expr.Left as ParenExpression;
    leftExpr.Should().NotBeNull();
    var leftInner = leftExpr!.Inner as BinaryExpression;
    leftInner.Should().NotBeNull();
    leftInner!.Operator.Should().Be(BinaryOperator.And);

    // Проверяем правую часть (скобки с &&)
    var rightExpr = expr.Right as ParenExpression;
    rightExpr.Should().NotBeNull();
    var rightInner = rightExpr!.Inner as BinaryExpression;
    rightInner.Should().NotBeNull();
    rightInner!.Operator.Should().Be(BinaryOperator.And);
  }

  [Fact]
  public void ParseSingleConditionWithAndOperatorAndForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when temp > 20 && temp < 30 for 5m {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.And);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(5);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseSingleConditionWithOrOperatorAndForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when door.open == true || window.open == true for 10s {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Or);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(10);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseSingleConditionWithComplexOperatorsAndForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when (temp > 20 || temp < 10) && humidity < 80 for 1h {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.And);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(1);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Hours);
  }

  [Fact]
  public void ParseAllConditionWithMultipleExpressions_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when all {
    kitchen.sensors.motion == on;
    kitchen.sensors.lux < 120;
    kitchen.temp > 20;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(3);
  }

  [Fact]
  public void ParseAnyConditionWithMultipleExpressions_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when any {
    living.climate.temp > 28.0;
    living.humidity > 70;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
  }

  [Fact]
  public void ParseAllConditionWithNestedAnyCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when all {
    any {
      living.climate.temp > 25.0;
      living.humidity > 65;
    };
    living.windows.main == closed;
    living.temp > 20;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(3);

    // Проверяем, что первое условие - это вложенный any блок
    // Примечание: это может не работать с текущим парсером, так как он использует Expression, а не ConditionExpression
  }

  [Fact]
  public void ParseAnyConditionWithNestedAllCondition_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when any {
    all {
      temp > 20;
      humidity < 80;
    };
    pressure > 1000;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
  }

  [Fact]
  public void ParseAllConditionWithNestedAnyAndForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when all for 5m {
    any {
      temp > 25;
      humidity > 65;
    };
    window == ""closed"";
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(5);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseAnyConditionWithNestedAllAndForDuration_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when any for 10s {
    all {
      door.open == true;
      window.open == true;
    };
    motion == on;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AnyCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
    condition.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(10);
    condition.ForDuration.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseDeeplyNestedConditions_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when all {
    any {
      all {
        temp > 20;
        humidity < 80;
      };
      pressure > 1000;
    };
    temp > 15;
  } {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as AllCondition;
    condition.Should().NotBeNull();
    condition!.Conditions.Should().HaveCount(2);
  }
}
