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
}
