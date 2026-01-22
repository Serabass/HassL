using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class ReferencesAndFunctionsTests
{
    [Fact]
    public void ParseReference_ShouldParseSinglePart()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test { do test(); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
        condition.Should().NotBeNull();
        var expr = condition!.Expression as ReferenceExpression;
        expr.Should().NotBeNull();
        expr!.Reference.Parts.Should().Equal("test");
    }

    [Fact]
    public void ParseReference_ShouldParseMultipleParts()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when home.room.device.entity { do test(); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
        condition.Should().NotBeNull();
        var expr = condition!.Expression as ReferenceExpression;
        expr.Should().NotBeNull();
        expr!.Reference.Parts.Should().Equal("home", "room", "device", "entity");
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithoutTarget()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do notify(); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Name.Should().Be("notify");
        action.FunctionCall.Target.Should().BeNull();
        action.FunctionCall.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithTarget()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do light.turn_on(); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Name.Should().Be("turn_on");
        action.FunctionCall.Target.Should().Be("light");
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithSingleArgument()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do notify(\"message\"); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithMultipleArguments()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do test.func(1, \"test\", true, 3.14); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Arguments.Should().HaveCount(4);
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithReferenceArgument()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do light.turn_on(home.room.device.entity); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Arguments.Should().HaveCount(1);
        var expr = action.FunctionCall.Arguments[0] as ReferenceExpression;
        expr.Should().NotBeNull();
        expr!.Reference.Parts.Should().Equal("home", "room", "device", "entity");
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithNestedFunctionCall()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do test.func(test.other(1)); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Arguments.Should().HaveCount(1);
        var expr = action.FunctionCall.Arguments[0] as FunctionCallExpression;
        expr.Should().NotBeNull();
    }

    [Fact]
    public void ParseFunctionCall_ShouldParseWithObjectLiteralArgument()
    {
        // Act
        var result = HassLanguageParser.Parse("automation \"Test\" { when test.value == 5 { do light.turn_on({ brightness: 70 }); } }");

        // Assert
        result.Automations.Should().HaveCount(1);
        var actionBlock = result.Automations[0].WhenClauses[0].Actions;
        var action = actionBlock.Statements[0] as DoAction;
        action.Should().NotBeNull();
        action!.FunctionCall.Arguments.Should().HaveCount(1);
        var expr = action.FunctionCall.Arguments[0] as LiteralExpression;
        expr.Should().NotBeNull();
        var objLiteral = expr!.Literal as ObjectLiteral;
        objLiteral.Should().NotBeNull();
    }
}
