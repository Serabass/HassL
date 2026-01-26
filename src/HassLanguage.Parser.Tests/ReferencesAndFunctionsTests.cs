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
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test {
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
    expr!.Reference.Parts.Should().Equal("test");
  }

  [Fact]
  public void ParseReference_ShouldParseMultipleParts()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when zone.area.device.entity {
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
    expr!.Reference.Parts.Should().Equal("zone", "area", "device", "entity");
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithoutTarget()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call notify();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("notify");
    action.FunctionCall.Target.Should().BeNull();
    action.FunctionCall.Arguments.Should().BeEmpty();
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithTarget()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("turn_on");
    action.FunctionCall.Target.Should().Be("light");
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithSingleArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call notify('message');
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithMultipleArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(1, 'test', true, 3.14);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(4);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithReferenceArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on(zone.area.device.entity);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var expr = action.FunctionCall.Arguments[0] as ReferenceExpression;
    expr.Should().NotBeNull();
    expr!.Reference.Parts.Should().Equal("zone", "area", "device", "entity");
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithNestedFunctionCall()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(test.other(1));
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var expr = action.FunctionCall.Arguments[0] as FunctionCallExpression;
    expr.Should().NotBeNull();
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseWithObjectLiteralArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on({ brightness: 70 });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var expr = action.FunctionCall.Arguments[0] as LiteralExpression;
    expr.Should().NotBeNull();
    var objLiteral = expr!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseFunctionWithArgumentsInBinaryExpression()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.func(1) == 5 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var binExpr = condition!.Expression as BinaryExpression;
    binExpr.Should().NotBeNull();
    var funcExpr = binExpr!.Left as FunctionCallExpression;
    funcExpr.Should().NotBeNull();
    funcExpr!.FunctionCall.Name.Should().Be("func");
    funcExpr.FunctionCall.Target.Should().Be("test");
    funcExpr.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseFunctionCallInInRangeExpression()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.func(1) in 10..20 {
    call test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var condition = result.Automations[0].WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var inRangeExpr = condition!.Expression as InRangeExpression;
    inRangeExpr.Should().NotBeNull();
    var funcExpr = inRangeExpr!.Left as FunctionCallExpression;
    funcExpr.Should().NotBeNull();
    funcExpr!.FunctionCall.Name.Should().Be("func");
    funcExpr.FunctionCall.Target.Should().Be("test");
    funcExpr.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseFunctionWithReferenceArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(sensor.temp);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("func");
    action.FunctionCall.Target.Should().Be("test");
    action.FunctionCall.Arguments.Should().HaveCount(1);
    var refExpr = action.FunctionCall.Arguments[0] as ReferenceExpression;
    refExpr.Should().NotBeNull();
    refExpr!.Reference.Parts.Should().Equal("sensor", "temp");
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseComplexFunctionCallWithMultipleArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on(zone.area.device.entity, { brightness: 70 }, true);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("turn_on");
    action.FunctionCall.Target.Should().Be("light");
    action.FunctionCall.Arguments.Should().HaveCount(3);

    // First argument - reference
    var refExpr = action.FunctionCall.Arguments[0] as ReferenceExpression;
    refExpr.Should().NotBeNull();
    refExpr!.Reference.Parts.Should().Equal("zone", "area", "device", "entity");

    // Second argument - object literal
    var objExpr = action.FunctionCall.Arguments[1] as LiteralExpression;
    objExpr.Should().NotBeNull();
    var objLiteral = objExpr!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();

    // Third argument - boolean
    var boolExpr = action.FunctionCall.Arguments[2] as LiteralExpression;
    boolExpr.Should().NotBeNull();
    var boolLiteral = boolExpr!.Literal as BooleanLiteral;
    boolLiteral.Should().NotBeNull();
    boolLiteral!.Value.Should().BeTrue();
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseNestedFunctionCallsInArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call http.telegram.post(format('/bot{0}/sendMessage', secret('TG_BOT_TOKEN')), json({ chat_id: secret('TG_CHAT_ID'), text: 'Hello' }));
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("post");
    action.FunctionCall.Target.Should().Be("http.telegram");
    action.FunctionCall.Arguments.Should().HaveCount(2);

    // First argument - nested function call (format)
    var formatExpr = action.FunctionCall.Arguments[0] as FunctionCallExpression;
    formatExpr.Should().NotBeNull();
    formatExpr!.FunctionCall.Name.Should().Be("format");
    formatExpr.FunctionCall.Target.Should().BeNull();
    formatExpr.FunctionCall.Arguments.Should().HaveCount(2);

    // Second argument of format - nested function call (secret)
    var secretExpr1 = formatExpr.FunctionCall.Arguments[1] as FunctionCallExpression;
    secretExpr1.Should().NotBeNull();
    secretExpr1!.FunctionCall.Name.Should().Be("secret");
    secretExpr1.FunctionCall.Arguments.Should().HaveCount(1);

    // Second argument - nested function call (json)
    var jsonExpr = action.FunctionCall.Arguments[1] as FunctionCallExpression;
    jsonExpr.Should().NotBeNull();
    jsonExpr!.FunctionCall.Name.Should().Be("json");
    jsonExpr.FunctionCall.Target.Should().BeNull();
    jsonExpr.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseFunctionCallInAllCondition()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when all {
    test.func1(1) in 10..20;
    test.func2(sensor.temp) > 25.0;
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

    // First condition - function call in range
    var inRangeExpr = condition.Conditions[0] as InRangeExpression;
    inRangeExpr.Should().NotBeNull();
    var funcExpr1 = inRangeExpr!.Left as FunctionCallExpression;
    funcExpr1.Should().NotBeNull();
    funcExpr1!.FunctionCall.Name.Should().Be("func1");
    funcExpr1.FunctionCall.Target.Should().Be("test");
    funcExpr1.FunctionCall.Arguments.Should().HaveCount(1);

    // Second condition - function call in binary expression
    var binExpr = condition.Conditions[1] as BinaryExpression;
    binExpr.Should().NotBeNull();
    var funcExpr2 = binExpr!.Left as FunctionCallExpression;
    funcExpr2.Should().NotBeNull();
    funcExpr2!.FunctionCall.Name.Should().Be("func2");
    funcExpr2.FunctionCall.Target.Should().Be("test");
    funcExpr2.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseFunctionCall_ShouldParseMultipleFunctionCallsInActionBlock()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on(light.ceiling, { brightness: 70 });
    call notify.telegram('Light on');
    call climate.set_mode(living.climate.ac, 'cool');
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(3);

    // First action
    var action1 = actionBlock.Statements[0] as CallAction;
    action1.Should().NotBeNull();
    action1!.FunctionCall.Name.Should().Be("turn_on");
    action1.FunctionCall.Target.Should().Be("light");

    // Second action
    var action2 = actionBlock.Statements[1] as CallAction;
    action2.Should().NotBeNull();
    action2!.FunctionCall.Name.Should().Be("telegram");
    action2.FunctionCall.Target.Should().Be("notify");

    // Third action
    var action3 = actionBlock.Statements[2] as CallAction;
    action3.Should().NotBeNull();
    action3!.FunctionCall.Name.Should().Be("set_mode");
    action3.FunctionCall.Target.Should().Be("climate");
  }
}
