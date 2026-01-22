using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class RangeTests
{
  [Theory]
  [InlineData("00:00..23:59", 0, 0, 23, 59)]
  [InlineData("08:00..18:00", 8, 0, 18, 0)]
  [InlineData("12:30..15:45", 12, 30, 15, 45)]
  [InlineData("06:00..09:30", 6, 0, 9, 30)]
  [InlineData("22:00..06:00", 22, 0, 6, 0)]
  public void ParseTimeRangeExpression_ShouldParseCorrectly(
    string input,
    int expectedStartHour,
    int expectedStartMinute,
    int expectedEndHour,
    int expectedEndMinute
  )
  {
    // Act
    var result = SpracheParser.ParseRangeExpression(input);

    // Assert
    result.Should().BeOfType<TimeRangeExpression>();
    var timeRange = (TimeRangeExpression)result;
    timeRange.Start.Hour.Should().Be(expectedStartHour);
    timeRange.Start.Minute.Should().Be(expectedStartMinute);
    timeRange.End.Hour.Should().Be(expectedEndHour);
    timeRange.End.Minute.Should().Be(expectedEndMinute);
  }

  [Theory]
  [InlineData("0..10", 0, 10)]
  [InlineData("5..25", 5, 25)]
  [InlineData("100..200", 100, 200)]
  public void ParseValueRangeExpression_WithIntLiterals_ShouldParseCorrectly(
    string input,
    int expectedStart,
    int expectedEnd
  )
  {
    // Act
    var result = SpracheParser.ParseRangeExpression(input);

    // Assert
    result.Should().BeOfType<ValueRangeExpression>();
    var valueRange = (ValueRangeExpression)result;
    valueRange.Start.Should().BeOfType<LiteralExpression>();
    valueRange.End.Should().BeOfType<LiteralExpression>();

    var startLiteral = (LiteralExpression)valueRange.Start;
    var endLiteral = (LiteralExpression)valueRange.End;

    startLiteral.Literal.Should().BeOfType<NumericLiteral>();
    endLiteral.Literal.Should().BeOfType<NumericLiteral>();

    var startNumeric = (NumericLiteral)startLiteral.Literal;
    var endNumeric = (NumericLiteral)endLiteral.Literal;

    startNumeric.IsFloat.Should().BeFalse();
    endNumeric.IsFloat.Should().BeFalse();
    startNumeric.Value.Should().Be(expectedStart);
    endNumeric.Value.Should().Be(expectedEnd);
  }

  [Theory]
  [InlineData("0.0..10.5", 0.0, 10.5)]
  [InlineData("5.5..25.75", 5.5, 25.75)]
  [InlineData("100.1..200.9", 100.1, 200.9)]
  public void ParseValueRangeExpression_WithFloatLiterals_ShouldParseCorrectly(
    string input,
    double expectedStart,
    double expectedEnd
  )
  {
    // Act
    var result = SpracheParser.ParseRangeExpression(input);

    // Assert
    result.Should().BeOfType<ValueRangeExpression>();
    var valueRange = (ValueRangeExpression)result;
    valueRange.Start.Should().BeOfType<LiteralExpression>();
    valueRange.End.Should().BeOfType<LiteralExpression>();

    var startLiteral = (LiteralExpression)valueRange.Start;
    var endLiteral = (LiteralExpression)valueRange.End;

    startLiteral.Literal.Should().BeOfType<NumericLiteral>();
    endLiteral.Literal.Should().BeOfType<NumericLiteral>();

    var startNumeric = (NumericLiteral)startLiteral.Literal;
    var endNumeric = (NumericLiteral)endLiteral.Literal;

    startNumeric.IsFloat.Should().BeTrue();
    endNumeric.IsFloat.Should().BeTrue();
    startNumeric.Value.Should().Be(expectedStart);
    endNumeric.Value.Should().Be(expectedEnd);
  }

  [Fact]
  public void ParseValueRangeExpression_WithReference_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseRangeExpression("temp..25");

    // Assert
    result.Should().BeOfType<ValueRangeExpression>();
    var valueRange = (ValueRangeExpression)result;
    valueRange.Start.Should().BeOfType<ReferenceExpression>();
    valueRange.End.Should().BeOfType<LiteralExpression>();

    var startRef = (ReferenceExpression)valueRange.Start;
    startRef.Reference.Parts.Should().Equal("temp");

    var endLiteral = (LiteralExpression)valueRange.End;
    endLiteral.Literal.Should().BeOfType<NumericLiteral>();
    var endNumeric = (NumericLiteral)endLiteral.Literal;
    endNumeric.Value.Should().Be(25);
  }

  [Fact]
  public void ParseValueRangeExpression_WithBothReferences_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseRangeExpression("minTemp..maxTemp");

    // Assert
    result.Should().BeOfType<ValueRangeExpression>();
    var valueRange = (ValueRangeExpression)result;
    valueRange.Start.Should().BeOfType<ReferenceExpression>();
    valueRange.End.Should().BeOfType<ReferenceExpression>();

    var startRef = (ReferenceExpression)valueRange.Start;
    var endRef = (ReferenceExpression)valueRange.End;

    startRef.Reference.Parts.Should().Equal("minTemp");
    endRef.Reference.Parts.Should().Equal("maxTemp");
  }

  [Fact]
  public void ParseInRangeExpression_WithTimeRange_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseExpression("time in 08:00..18:00");

    // Assert
    result.Should().BeOfType<InRangeExpression>();
    var inRange = (InRangeExpression)result;
    inRange.Left.Should().BeOfType<ReferenceExpression>();
    inRange.Range.Should().BeOfType<TimeRangeExpression>();

    var leftRef = (ReferenceExpression)inRange.Left;
    leftRef.Reference.Parts.Should().Equal("time");

    var timeRange = (TimeRangeExpression)inRange.Range;
    timeRange.Start.Hour.Should().Be(8);
    timeRange.Start.Minute.Should().Be(0);
    timeRange.End.Hour.Should().Be(18);
    timeRange.End.Minute.Should().Be(0);
  }

  [Fact]
  public void ParseInRangeExpression_WithValueRange_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseExpression("temp in 20..25");

    // Assert
    result.Should().BeOfType<InRangeExpression>();
    var inRange = (InRangeExpression)result;
    inRange.Left.Should().BeOfType<ReferenceExpression>();
    inRange.Range.Should().BeOfType<ValueRangeExpression>();

    var leftRef = (ReferenceExpression)inRange.Left;
    leftRef.Reference.Parts.Should().Equal("temp");

    var valueRange = (ValueRangeExpression)inRange.Range;
    valueRange.Start.Should().BeOfType<LiteralExpression>();
    valueRange.End.Should().BeOfType<LiteralExpression>();

    var startLiteral = (LiteralExpression)valueRange.Start;
    var endLiteral = (LiteralExpression)valueRange.End;

    var startNumeric = (NumericLiteral)startLiteral.Literal;
    var endNumeric = (NumericLiteral)endLiteral.Literal;

    startNumeric.Value.Should().Be(20);
    endNumeric.Value.Should().Be(25);
  }

  [Fact]
  public void ParseInRangeExpression_WithComplexLeftSide_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseExpression("sensor.temperature in 20..25");

    // Assert
    result.Should().BeOfType<InRangeExpression>();
    var inRange = (InRangeExpression)result;
    inRange.Left.Should().BeOfType<ReferenceExpression>();
    inRange.Range.Should().BeOfType<ValueRangeExpression>();

    var leftRef = (ReferenceExpression)inRange.Left;
    leftRef.Reference.Parts.Should().Equal("sensor", "temperature");
  }

  [Fact]
  public void ParseInRangeExpression_WithParenExpression_ShouldParseCorrectly()
  {
    // Act
    var result = SpracheParser.ParseExpression("(temp) in 20..25");

    // Assert
    result.Should().BeOfType<InRangeExpression>();
    var inRange = (InRangeExpression)result;
    inRange.Left.Should().BeOfType<ParenExpression>();
    inRange.Range.Should().BeOfType<ValueRangeExpression>();
  }
}
