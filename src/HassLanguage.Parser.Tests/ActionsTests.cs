using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class ActionsTests
{
  [Fact]
  public void ParseDoAction_ShouldParseFunctionCall()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do test.func(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(1);
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("func");
    action.FunctionCall.Target.Should().Be("test");
  }

  [Fact]
  public void ParseDoAction_ShouldParseFunctionCallWithoutTarget()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do notify(); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("notify");
    action.FunctionCall.Target.Should().BeNull();
  }

  [Fact]
  public void ParseDoAction_ShouldParseFunctionCallWithArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do test.func(1, \"test\", true); } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseWaitAction_ShouldParseCorrectly()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { wait test.value == 0 for 10s; } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(1);
    var action = actionBlock.Statements[0] as WaitAction;
    action.Should().NotBeNull();
    action!.Condition.Should().NotBeNull();
    action.ForDuration.Should().NotBeNull();
    action.ForDuration!.Value.Should().Be(10);
    action.ForDuration.Unit.Should().Be(DurationUnit.Seconds);
  }

  [Fact]
  public void ParseWaitAction_ShouldParseWithTimeout()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { wait test.value == 0 for 10s timeout 5m; } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as WaitAction;
    action.Should().NotBeNull();
    action!.Timeout.Should().NotBeNull();
    action.Timeout!.Value.Should().Be(5);
    action.Timeout.Unit.Should().Be(DurationUnit.Minutes);
  }

  [Fact]
  public void ParseActionBlock_ShouldParseMultipleActions()
  {
    // Act
    var result = HassLanguageParser.Parse(
      "automation \"Test\" { when test.value == 5 { do test.func1(); do test.func2(); wait test.value == 0 for 10s; } }"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(3);
    actionBlock.Statements[0].Should().BeOfType<DoAction>();
    actionBlock.Statements[1].Should().BeOfType<DoAction>();
    actionBlock.Statements[2].Should().BeOfType<WaitAction>();
  }
}
