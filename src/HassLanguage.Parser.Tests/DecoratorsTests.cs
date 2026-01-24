using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class DecoratorsTests
{
  [Fact]
  public void ParseDecorator_ShouldParseWithStringArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode('restart') automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Name.Should().Be("mode");
    result.Automations[0].Decorators[0].Arguments.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments[0].Should().BeOfType<StringDecoratorArgument>();
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithIdentifierArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(restart) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result
      .Automations[0]
      .Decorators[0]
      .Arguments[0]
      .Should()
      .BeOfType<IdentifierDecoratorArgument>();
    var arg = result.Automations[0].Decorators[0].Arguments[0] as IdentifierDecoratorArgument;
    arg!.Value.Should().Be("restart");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithDurationArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@cooldown(10s) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments[0].Should().BeOfType<DurationDecoratorArgument>();
    var arg = result.Automations[0].Decorators[0].Arguments[0] as DurationDecoratorArgument;
    arg!.Value.Value.Should().Be(10);
    arg.Value.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithIntArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@retry(3) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments[0].Should().BeOfType<IntDecoratorArgument>();
    var arg = result.Automations[0].Decorators[0].Arguments[0] as IntDecoratorArgument;
    arg!.Value.Should().Be(3);
  }

  [Fact]
  public void ParseDecorator_ShouldParseMultipleArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test('arg1', 10s, 5) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseDecoratorList_ShouldParseMultipleDecorators()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(restart) @cooldown(10s) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(2);
    result.Automations[0].Decorators[0].Name.Should().Be("mode");
    result.Automations[0].Decorators[1].Name.Should().Be("cooldown");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnWhenClause()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  @edge(rising) when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].WhenClauses[0].Decorators.Should().HaveCount(1);
    result.Automations[0].WhenClauses[0].Decorators[0].Name.Should().Be("edge");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnHome()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@enabled(true) home 'TestHome' test {
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Decorators[0].Name.Should().Be("enabled");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnArea()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  @priority(high) area 'TestArea' area {
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Areas[0].Decorators[0].Name.Should().Be("priority");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    @enabled(true) device 'TestDevice' device {
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].Decorators[0].Name.Should().Be("enabled");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithAllDurationUnits()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test1(5s) @test2(10m) @test3(2h) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(3);

    var arg1 = result.Automations[0].Decorators[0].Arguments[0] as DurationDecoratorArgument;
    arg1.Should().NotBeNull();
    arg1!.Value.Value.Should().Be(5);
    arg1.Value.Unit.Should().Be(DurationUnit.Seconds);

    var arg2 = result.Automations[0].Decorators[1].Arguments[0] as DurationDecoratorArgument;
    arg2.Should().NotBeNull();
    arg2!.Value.Value.Should().Be(10);
    arg2.Value.Unit.Should().Be(DurationUnit.Minutes);

    var arg3 = result.Automations[0].Decorators[2].Arguments[0] as DurationDecoratorArgument;
    arg3.Should().NotBeNull();
    arg3!.Value.Value.Should().Be(2);
    arg3.Value.Unit.Should().Be(DurationUnit.Hours);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithLargeIntArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@retry(999) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as IntDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be(999);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithZeroIntArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@retry(0) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as IntDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be(0);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithEmptyStringArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode('') automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as StringDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be("");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithLongStringArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode('very long string with spaces and special chars !@#$') automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as StringDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be("very long string with spaces and special chars !@#$");
  }

  [Fact]
  public void ParseDecorator_ShouldParseMultipleDecoratorsOnWhenClause()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  @edge(rising) @debounce(2s) when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].WhenClauses[0].Decorators.Should().HaveCount(2);
    result.Automations[0].WhenClauses[0].Decorators[0].Name.Should().Be("edge");
    result.Automations[0].WhenClauses[0].Decorators[1].Name.Should().Be("debounce");
  }

  [Fact]
  public void ParseDecorator_ShouldParseMultipleDecoratorsOnHome()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@enabled(true) @priority(high) home 'TestHome' test {
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Decorators.Should().HaveCount(2);
    result.Homes[0].Decorators[0].Name.Should().Be("enabled");
    result.Homes[0].Decorators[1].Name.Should().Be("priority");
  }

  [Fact]
  public void ParseDecorator_ShouldParseMultipleDecoratorsOnRoom()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  @priority(high) @enabled(true) area 'TestArea' area {
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Decorators.Should().HaveCount(2);
    result.Homes[0].Areas[0].Decorators[0].Name.Should().Be("priority");
    result.Homes[0].Areas[0].Decorators[1].Name.Should().Be("enabled");
  }

  [Fact]
  public void ParseDecorator_ShouldParseMultipleDecoratorsOnDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    @enabled(true) @priority(low) device 'TestDevice' device {
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].Decorators.Should().HaveCount(2);
    result.Homes[0].Areas[0].Devices[0].Decorators[0].Name.Should().Be("enabled");
    result.Homes[0].Areas[0].Devices[0].Decorators[1].Name.Should().Be("priority");
  }

  [Fact]
  public void ParseDecorator_ShouldParseComplexCombination()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(restart) @cooldown(10s) @retry(3) automation 'Test' {
  @edge(rising) @debounce(2s) when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(3);
    result.Automations[0].WhenClauses[0].Decorators.Should().HaveCount(2);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithMixedArgumentTypesInOneDecorator()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test('string', identifier, 10s, 42) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments.Should().HaveCount(4);

    result.Automations[0].Decorators[0].Arguments[0].Should().BeOfType<StringDecoratorArgument>();
    result
      .Automations[0]
      .Decorators[0]
      .Arguments[1]
      .Should()
      .BeOfType<IdentifierDecoratorArgument>();
    result.Automations[0].Decorators[0].Arguments[2].Should().BeOfType<DurationDecoratorArgument>();
    result.Automations[0].Decorators[0].Arguments[3].Should().BeOfType<IntDecoratorArgument>();
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithStringArgumentContainingQuotes()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode('test with ""double quotes""') automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as StringDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Contain("double quotes");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithIdentifierContainingUnderscores()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test_mode(restart_mode) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Name.Should().Be("test_mode");
    var arg = result.Automations[0].Decorators[0].Arguments[0] as IdentifierDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be("restart_mode");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithDoubleQuotedString()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(""restart"") automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var arg = result.Automations[0].Decorators[0].Arguments[0] as StringDecoratorArgument;
    arg.Should().NotBeNull();
    arg!.Value.Should().Be("restart");
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithManyArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test('a', 'b', 'c', 'd', 'e') automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments.Should().HaveCount(5);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithSpacesInArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@test(  'value'  ,  10s  ,  42  ) automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators[0].Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithNewlinesBetweenDecorators()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(restart)
@cooldown(10s)
automation 'Test' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(2);
  }

  [Fact]
  public void ParseDecorator_ShouldParseWithComplexNestedStructure()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"@mode(restart)
@cooldown(10s)
automation 'Test' {
  @edge(rising)
  @debounce(2s)
  when test.value == 5 {
    do test();
  }
  @edge(falling)
  when test.value == 10 {
    do test2();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(2);
    result.Automations[0].WhenClauses.Should().HaveCount(2);
    result.Automations[0].WhenClauses[0].Decorators.Should().HaveCount(2);
    result.Automations[0].WhenClauses[1].Decorators.Should().HaveCount(1);
  }
}
