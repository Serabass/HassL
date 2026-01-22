using FluentAssertions;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class BasicParsersTests
{
  [Theory]
  [InlineData("test", "test")]
  [InlineData("test123", "test123")]
  [InlineData("_test", "_test")]
  [InlineData("test_var", "test_var")]
  [InlineData("TestVar", "TestVar")]
  public void ParseIdentifier_ShouldParseValidIdentifiers(string input, string expected)
  {
    // Act
    var result = HassLanguageParser.Parse($"home \"Test\" {expected} {{ }}");

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be(expected);
  }

  [Theory]
  [InlineData("\"simple\"", "simple")]
  [InlineData("\"with spaces\"", "with spaces")]
  [InlineData("\"with\\\"quotes\"", "with\"quotes")]
  [InlineData("\"\"", "")]
  public void ParseStringLiteral_ShouldParseValidStrings(string input, string expected)
  {
    // Act
    var result = HassLanguageParser.Parse($"home {input} test {{ }}");

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].DisplayName.Should().Be(expected);
  }

  [Theory]
  [InlineData("123", 123)]
  [InlineData("0", 0)]
  [InlineData("999", 999)]
  public void ParseIntLiteral_ShouldParseValidIntegers(string input, int expected)
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when {input} == {expected} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
  }

  [Theory]
  [InlineData("123.45", 123.45)]
  [InlineData("0.5", 0.5)]
  [InlineData("999.999", 999.999)]
  public void ParseFloatLiteral_ShouldParseValidFloats(string input, double expected)
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when {input} > 0.0 {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
  }

  [Theory]
  [InlineData("true", true)]
  [InlineData("false", false)]
  public void ParseBooleanLiteral_ShouldParseValidBooleans(string input, bool expected)
  {
    // Act
    var result = HassLanguageParser.Parse(
      $"automation \"Test\" {{ when {input} == {expected.ToString().ToLower()} {{ do test(); }} }}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
  }

  [Fact]
  public void Parse_ShouldIgnoreLineComments()
  {
    // Arrange
    var input =
      @"
// This is a comment
home ""Test"" test {
  // Another comment
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreBlockComments()
  {
    // Arrange
    var input =
      @"
/* This is a block comment */
home ""Test"" test {
  /* Another block comment */
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldHandleWhitespace()
  {
    // Arrange
    var input = "home   \"Test\"   test   {   }";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }
}
