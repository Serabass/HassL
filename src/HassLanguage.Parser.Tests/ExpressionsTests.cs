using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class ExpressionsTests
{
  [Theory]
  [InlineData("==", BinaryOperator.Equals)]
  [InlineData("!=", BinaryOperator.NotEquals)]
  [InlineData(">", BinaryOperator.Greater)]
  [InlineData("<", BinaryOperator.Less)]
  [InlineData(">=", BinaryOperator.GreaterOrEqual)]
  [InlineData("<=", BinaryOperator.LessOrEqual)]
  public void ParseComparisonExpression_ShouldParseOperators(string op, BinaryOperator expectedOp)
  {
    // Act
    var result = HassLanguageParser.Parse(
      $@"automation 'Test' {{
  when test.value {op} 5 {{
    call test();
  }}
}}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(expectedOp);
  }

  [Fact]
  public void ParseLogicalAnd_ShouldParseAndOperator()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value > 5 && test.value < 10 {
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
  }

  [Fact]
  public void ParseLogicalOr_ShouldParseOrOperator()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value > 5 || test.value < 0 {
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
  }

  [Fact]
  public void ParseParenthesizedExpression_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when (test.value > 5) {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as ParenExpression;
    expr.Should().NotBeNull();
  }

  [Fact]
  public void ParseReferenceExpression_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.device.entity {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as ReferenceExpression;
    expr.Should().NotBeNull();
    expr!.Reference.Parts.Should().Equal("test", "device", "entity");
  }

  [Fact]
  public void ParseFunctionCallExpression_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.func(1, 2) {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as FunctionCallExpression;
    expr.Should().NotBeNull();
    expr!.FunctionCall.Name.Should().Be("func");
    expr.FunctionCall.Target.Should().Be("test");
    expr.FunctionCall.Arguments.Should().HaveCount(2);
  }

  [Fact]
  public void ParseInRangeExpression_ShouldParseTimeRange()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when time in 08:00..18:00 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as InRangeExpression;
    expr.Should().NotBeNull();
    var range = expr!.Range as TimeRangeExpression;
    range.Should().NotBeNull();
    range!.Start.Hour.Should().Be(8);
    range.Start.Minute.Should().Be(0);
    range.End.Hour.Should().Be(18);
    range.End.Minute.Should().Be(0);
  }

  [Fact]
  public void ParseInRangeExpression_ShouldParseValueRange()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when temp in 20..25 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as InRangeExpression;
    expr.Should().NotBeNull();
    var range = expr!.Range as ValueRangeExpression;
    range.Should().NotBeNull();
  }
}
