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
      "@mode(\"restart\") automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "@mode(restart) automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "@cooldown(10s) automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "@retry(3) automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "@test(\"arg1\", 10s, 5) automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "@mode(restart) @cooldown(10s) automation \"Test\" { when test.value == 5 { do test(); } }"
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
      "automation \"Test\" { @edge(rising) when test.value == 5 { do test(); } }"
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
    var result = HassLanguageParser.Parse("@enabled(true) home \"TestHome\" test { }");

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Decorators[0].Name.Should().Be("enabled");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnRoom()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "home \"TestHome\" test { @priority(high) room \"TestRoom\" room { } }"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Decorators[0].Name.Should().Be("priority");
  }

  [Fact]
  public void ParseDecorator_ShouldParseOnDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "home \"TestHome\" test { room \"TestRoom\" room { @enabled(true) device \"TestDevice\" device { } } }"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Devices[0].Decorators.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Devices[0].Decorators[0].Name.Should().Be("enabled");
  }
}
