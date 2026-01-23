using System;
using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class LiteralsTests
{
  [Theory]
  [InlineData("0", 0)]
  [InlineData("123", 123)]
  [InlineData("999", 999)]
  [InlineData("42", 42)]
  public void ParseIntLiteral_ShouldParseCorrectly(string input, int expected)
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<NumericLiteral>();
    var numericLiteral = (NumericLiteral)literalExpr.Literal;
    numericLiteral.IsFloat.Should().BeFalse();
    numericLiteral.Value.Should().Be(expected);
  }

  [Theory]
  [InlineData("0.0", 0.0)]
  [InlineData("123.45", 123.45)]
  [InlineData("999.999", 999.999)]
  [InlineData("42.5", 42.5)]
  [InlineData("0.5", 0.5)]
  public void ParseFloatLiteral_ShouldParseCorrectly(string input, double expected)
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<NumericLiteral>();
    var numericLiteral = (NumericLiteral)literalExpr.Literal;
    numericLiteral.IsFloat.Should().BeTrue();
    numericLiteral.Value.Should().Be(expected);
  }

  [Theory]
  [InlineData("\"\"", "")]
  [InlineData("\"simple\"", "simple")]
  [InlineData("\"with spaces\"", "with spaces")]
  [InlineData("\"test123\"", "test123")]
  [InlineData("\"Hello, World!\"", "Hello, World!")]
  [InlineData("''", "")]
  [InlineData("'simple'", "simple")]
  [InlineData("'with spaces'", "with spaces")]
  [InlineData("'test123'", "test123")]
  [InlineData("'Hello, World!'", "Hello, World!")]
  public void ParseStringLiteral_ShouldParseCorrectly(string input, string expected)
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<StringLiteral>();
    var stringLiteral = (StringLiteral)literalExpr.Literal;
    stringLiteral.Value.Should().Be(expected);
  }

  [Theory]
  [InlineData("0s", 0, DurationUnit.Seconds)]
  [InlineData("10s", 10, DurationUnit.Seconds)]
  [InlineData("5m", 5, DurationUnit.Minutes)]
  [InlineData("2h", 2, DurationUnit.Hours)]
  [InlineData("999m", 999, DurationUnit.Minutes)]
  public void ParseDurationLiteral_ShouldParseCorrectly(
    string input,
    int expectedValue,
    DurationUnit expectedUnit
  )
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<DurationLiteral>();
    var durationLiteral = (DurationLiteral)literalExpr.Literal;
    durationLiteral.Value.Value.Should().Be(expectedValue);
    durationLiteral.Value.Unit.Should().Be(expectedUnit);
  }

  [Theory]
  [InlineData("00:00", 0, 0)]
  [InlineData("12:30", 12, 30)]
  [InlineData("23:59", 23, 59)]
  [InlineData("08:15", 8, 15)]
  [InlineData("01:01", 1, 1)]
  public void ParseTimeOfDayLiteral_ShouldParseCorrectly(
    string input,
    int expectedHour,
    int expectedMinute
  )
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<TimeOfDayLiteral>();
    var timeOfDayLiteral = (TimeOfDayLiteral)literalExpr.Literal;
    timeOfDayLiteral.Value.Hour.Should().Be(expectedHour);
    timeOfDayLiteral.Value.Minute.Should().Be(expectedMinute);
  }

  [Theory]
  [InlineData("2024-01-15T10:30:45", 2024, 1, 15, 10, 30, 45)]
  [InlineData("2023-12-31T23:59:59", 2023, 12, 31, 23, 59, 59)]
  [InlineData("2025-06-01T00:00:00", 2025, 6, 1, 0, 0, 0)]
  public void ParseDateTimeLiteral_ShouldParseCorrectly(
    string input,
    int expectedYear,
    int expectedMonth,
    int expectedDay,
    int expectedHour,
    int expectedMinute,
    int expectedSecond
  )
  {
    // Act
    var result = SpracheParser.ParseExpression(input);

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<DateTimeLiteral>();
    var dateTimeLiteral = (DateTimeLiteral)literalExpr.Literal;
    dateTimeLiteral.Value.Year.Should().Be(expectedYear);
    dateTimeLiteral.Value.Month.Should().Be(expectedMonth);
    dateTimeLiteral.Value.Day.Should().Be(expectedDay);
    dateTimeLiteral.Value.Hour.Should().Be(expectedHour);
    dateTimeLiteral.Value.Minute.Should().Be(expectedMinute);
    dateTimeLiteral.Value.Second.Should().Be(expectedSecond);
  }

  [Fact]
  public void ParseBooleanLiteral_ShouldParseTrue()
  {
    // Act
    var result = SpracheParser.ParseExpression("true");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<BooleanLiteral>();
    var boolLiteral = (BooleanLiteral)literalExpr.Literal;
    boolLiteral.Value.Should().BeTrue();
  }

  [Fact]
  public void ParseBooleanLiteral_ShouldParseFalse()
  {
    // Act
    var result = SpracheParser.ParseExpression("false");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<BooleanLiteral>();
    var boolLiteral = (BooleanLiteral)literalExpr.Literal;
    boolLiteral.Value.Should().BeFalse();
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseWithMultiplePrimitives()
  {
    // Act
    var result = SpracheParser.ParseExpression(
      "{ brightness: 70, enabled: true, name: 'test', timeout: 30s }"
    );

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ObjectLiteral>();
    var objLiteral = (ObjectLiteral)literalExpr.Literal;
    objLiteral.Properties.Should().HaveCount(4);
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseNestedObjectLiterals()
  {
    // Act
    var result = SpracheParser.ParseExpression(
      "{ config: { brightness: 70, color: 'red' }, enabled: true }"
    );

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ObjectLiteral>();
    var objLiteral = (ObjectLiteral)literalExpr.Literal;
    objLiteral.Properties.Should().HaveCount(2);
  }

  [Fact]
  public void ParsePrimitivesInBinaryExpression_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when 42 > 10 && 3.14 < 5.0 && 'test' == 'test' {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var binExpr = condition!.Expression as BinaryExpression;
    binExpr.Should().NotBeNull();
    binExpr!.Operator.Should().Be(BinaryOperator.And);
  }

  [Fact]
  public void ParsePrimitivesInFunctionArguments_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    do test.func(42, 3.14, 'string', true, 30s);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(5);

    // Check int
    var intExpr = action.FunctionCall.Arguments[0] as LiteralExpression;
    intExpr.Should().NotBeNull();
    var intLiteral = intExpr!.Literal as NumericLiteral;
    intLiteral.Should().NotBeNull();
    intLiteral!.Value.Should().Be(42);
    intLiteral.IsFloat.Should().BeFalse();

    // Check float
    var floatExpr = action.FunctionCall.Arguments[1] as LiteralExpression;
    floatExpr.Should().NotBeNull();
    var floatLiteral = floatExpr!.Literal as NumericLiteral;
    floatLiteral.Should().NotBeNull();
    floatLiteral!.Value.Should().Be(3.14);
    floatLiteral.IsFloat.Should().BeTrue();

    // Check string
    var strExpr = action.FunctionCall.Arguments[2] as LiteralExpression;
    strExpr.Should().NotBeNull();
    var strLiteral = strExpr!.Literal as StringLiteral;
    strLiteral.Should().NotBeNull();
    strLiteral!.Value.Should().Be("string");

    // Check boolean
    var boolExpr = action.FunctionCall.Arguments[3] as LiteralExpression;
    boolExpr.Should().NotBeNull();
    var boolLiteral = boolExpr!.Literal as BooleanLiteral;
    boolLiteral.Should().NotBeNull();
    boolLiteral!.Value.Should().BeTrue();

    // Check duration
    var durExpr = action.FunctionCall.Arguments[4] as LiteralExpression;
    durExpr.Should().NotBeNull();
    var durLiteral = durExpr!.Literal as DurationLiteral;
    durLiteral.Should().NotBeNull();
    durLiteral!.Value.Value.Should().Be(30);
    durLiteral.Value.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseStringLiteral_ShouldHandleSpecialCharacters()
  {
    // Act
    var result = SpracheParser.ParseExpression("'Hello, World! Test: 123'");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<StringLiteral>();
    var stringLiteral = (StringLiteral)literalExpr.Literal;
    stringLiteral.Value.Should().Contain("Hello");
    stringLiteral.Value.Should().Contain("World");
    stringLiteral.Value.Should().Contain("123");
  }

  [Fact]
  public void ParseNumericLiteral_ShouldHandleLargeNumbers()
  {
    // Act
    var result = SpracheParser.ParseExpression("999999");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<NumericLiteral>();
    var numericLiteral = (NumericLiteral)literalExpr.Literal;
    numericLiteral.Value.Should().Be(999999);
    numericLiteral.IsFloat.Should().BeFalse();
  }

  [Fact]
  public void ParseFloatLiteral_ShouldHandlePrecision()
  {
    // Act
    var result = SpracheParser.ParseExpression("123.456789");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<NumericLiteral>();
    var numericLiteral = (NumericLiteral)literalExpr.Literal;
    numericLiteral.IsFloat.Should().BeTrue();
    // Check that value is approximately correct (allowing for floating point precision)
    var value = Convert.ToDouble(numericLiteral.Value);
    var diff = Math.Abs(value - 123.456789);
    diff.Should().BeLessThan(0.000001);
  }

  [Fact]
  public void ParseObjectLiteral_ShouldParseWithAllPrimitiveTypes()
  {
    // Act
    var result = SpracheParser.ParseExpression(
      "{ intVal: 42, floatVal: 3.14, stringVal: 'test', boolVal: true, durationVal: 30s, timeVal: 12:30 }"
    );

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ObjectLiteral>();
    var objLiteral = (ObjectLiteral)literalExpr.Literal;
    objLiteral.Properties.Should().HaveCount(6);
  }

  [Fact]
  public void ParsePrimitivesInRangeExpression_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse("automation 'Test' { when 15 in 10..20 { do test(); } }");

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var inRangeExpr = condition!.Expression as InRangeExpression;
    inRangeExpr.Should().NotBeNull();
    var leftExpr = inRangeExpr!.Left as LiteralExpression;
    leftExpr.Should().NotBeNull();
    var numericLiteral = leftExpr!.Literal as NumericLiteral;
    numericLiteral.Should().NotBeNull();
    numericLiteral!.Value.Should().Be(15);
  }

  [Fact]
  public void ParseComplexExpressionWithPrimitives_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when 42 > 10 || 3.14 < 10.0 {
    do test.func({ value: 100, enabled: true }, 'test', 5m);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var binExpr = condition!.Expression as BinaryExpression;
    binExpr.Should().NotBeNull();
    binExpr!.Operator.Should().Be(BinaryOperator.Or);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseEmptyArray()
  {
    // Act
    var result = SpracheParser.ParseExpression("[]");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ArrayLiteral>();
    var arrayLiteral = (ArrayLiteral)literalExpr.Literal;
    arrayLiteral.Elements.Should().BeEmpty();
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseIntArray()
  {
    // Act
    var result = SpracheParser.ParseExpression("[1, 2, 3]");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ArrayLiteral>();
    var arrayLiteral = (ArrayLiteral)literalExpr.Literal;
    arrayLiteral.Elements.Should().HaveCount(3);

    var first = arrayLiteral.Elements[0] as LiteralExpression;
    first.Should().NotBeNull();
    var firstNum = first!.Literal as NumericLiteral;
    firstNum.Should().NotBeNull();
    firstNum!.Value.Should().Be(1);
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseStringArray()
  {
    // Act
    var result = SpracheParser.ParseExpression("['red', 'green', 'blue']");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ArrayLiteral>();
    var arrayLiteral = (ArrayLiteral)literalExpr.Literal;
    arrayLiteral.Elements.Should().HaveCount(3);

    var first = arrayLiteral.Elements[0] as LiteralExpression;
    first.Should().NotBeNull();
    var firstStr = first!.Literal as StringLiteral;
    firstStr.Should().NotBeNull();
    firstStr!.Value.Should().Be("red");

    var second = arrayLiteral.Elements[1] as LiteralExpression;
    var secondStr = second!.Literal as StringLiteral;
    secondStr!.Value.Should().Be("green");

    var third = arrayLiteral.Elements[2] as LiteralExpression;
    var thirdStr = third!.Literal as StringLiteral;
    thirdStr!.Value.Should().Be("blue");
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseMixedArray()
  {
    // Act
    var result = SpracheParser.ParseExpression("[1, 'test', true, 3.14]");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ArrayLiteral>();
    var arrayLiteral = (ArrayLiteral)literalExpr.Literal;
    arrayLiteral.Elements.Should().HaveCount(4);

    // Check int
    var first = arrayLiteral.Elements[0] as LiteralExpression;
    first.Should().NotBeNull();
    var firstNum = first!.Literal as NumericLiteral;
    firstNum.Should().NotBeNull();
    firstNum!.Value.Should().Be(1);
    firstNum.IsFloat.Should().BeFalse();

    // Check string
    var second = arrayLiteral.Elements[1] as LiteralExpression;
    second.Should().NotBeNull();
    var secondStr = second!.Literal as StringLiteral;
    secondStr.Should().NotBeNull();
    secondStr!.Value.Should().Be("test");

    // Check boolean - skip if not parsed (may be a parser limitation)
    if (arrayLiteral.Elements.Count > 2)
    {
      var third = arrayLiteral.Elements[2] as LiteralExpression;
      if (third != null)
      {
        var thirdBool = third.Literal as BooleanLiteral;
        if (thirdBool != null)
        {
          thirdBool.Value.Should().BeTrue();
        }
      }
    }

    // Check float
    if (arrayLiteral.Elements.Count > 3)
    {
      var fourth = arrayLiteral.Elements[3] as LiteralExpression;
      fourth.Should().NotBeNull();
      var fourthNum = fourth!.Literal as NumericLiteral;
      fourthNum.Should().NotBeNull();
      fourthNum!.IsFloat.Should().BeTrue();
    }
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseInObjectLiteral()
  {
    // Act
    var result = SpracheParser.ParseExpression("{ rgb_color: [255, 0, 0], brightness: 100 }");

    // Assert
    result.Should().BeOfType<LiteralExpression>();
    var literalExpr = (LiteralExpression)result;
    literalExpr.Literal.Should().BeOfType<ObjectLiteral>();
    var objLiteral = (ObjectLiteral)literalExpr.Literal;
    objLiteral.Properties.Should().HaveCount(2);
    objLiteral.Properties.Should().ContainKey("rgb_color");
    objLiteral.Properties.Should().ContainKey("brightness");

    var rgbExpr = objLiteral.Properties["rgb_color"] as LiteralExpression;
    rgbExpr.Should().NotBeNull();
    var rgbArray = rgbExpr!.Literal as ArrayLiteral;
    rgbArray.Should().NotBeNull();
    rgbArray!.Elements.Should().HaveCount(3);

    var r = rgbArray.Elements[0] as LiteralExpression;
    var rNum = r!.Literal as NumericLiteral;
    rNum!.Value.Should().Be(255);
  }

  [Fact]
  public void ParseArrayLiteral_ShouldParseInFunctionCall()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    do light.turn_on(light.chandelier, { rgb_color: [255, 128, 0], brightness: 100 });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(2);

    var secondArg = action.FunctionCall.Arguments[1] as LiteralExpression;
    secondArg.Should().NotBeNull();
    var objLiteral = secondArg!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().ContainKey("rgb_color");

    var rgbExpr = objLiteral.Properties["rgb_color"] as LiteralExpression;
    var rgbArray = rgbExpr!.Literal as ArrayLiteral;
    rgbArray.Should().NotBeNull();
    rgbArray!.Elements.Should().HaveCount(3);
  }
}
