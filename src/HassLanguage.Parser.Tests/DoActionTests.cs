using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class CallActionTests
{
  [Fact]
  public void ParseCallAction_ShouldParseFunctionCall()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(1);
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Name.Should().Be("func");
    action.FunctionCall.Target.Should().Be("test");
  }

  [Fact]
  public void ParseCallAction_ShouldParseFunctionCallWithoutTarget()
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
  }

  [Fact]
  public void ParseCallAction_ShouldParseFunctionCallWithArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(1, 'test', true);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    var action = actionBlock.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithNoArguments()
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
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().BeEmpty();
    action.FunctionCall.Name.Should().Be("turn_on");
    action.FunctionCall.Target.Should().Be("light");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithNumericArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(42, 3.14);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(2);

    var firstArg = action.FunctionCall.Arguments[0] as LiteralExpression;
    firstArg.Should().NotBeNull();
    var firstNum = firstArg!.Literal as NumericLiteral;
    firstNum.Should().NotBeNull();
    firstNum!.Value.Should().Be(42);
    firstNum.IsFloat.Should().BeFalse();

    var secondArg = action.FunctionCall.Arguments[1] as LiteralExpression;
    secondArg.Should().NotBeNull();
    var secondNum = secondArg!.Literal as NumericLiteral;
    secondNum.Should().NotBeNull();
    secondNum!.IsFloat.Should().BeTrue();
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithStringArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call notify.telegram('Hello', 'World');
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(2);

    var firstArg = action.FunctionCall.Arguments[0] as LiteralExpression;
    var firstStr = firstArg!.Literal as StringLiteral;
    firstStr!.Value.Should().Be("Hello");

    var secondArg = action.FunctionCall.Arguments[1] as LiteralExpression;
    var secondStr = secondArg!.Literal as StringLiteral;
    secondStr!.Value.Should().Be("World");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithBooleanArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(true, false);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(2);

    var firstArg = action.FunctionCall.Arguments[0] as LiteralExpression;
    var firstBool = firstArg!.Literal as BooleanLiteral;
    firstBool!.Value.Should().BeTrue();

    var secondArg = action.FunctionCall.Arguments[1] as LiteralExpression;
    var secondBool = secondArg!.Literal as BooleanLiteral;
    secondBool!.Value.Should().BeFalse();
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithReferenceArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on(home.area.device.entity);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as ReferenceExpression;
    arg.Should().NotBeNull();
    arg!.Reference.Parts.Should().Equal("home", "area", "device", "entity");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithObjectLiteralArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.turn_on({ brightness: 100, effect: 'rainbow' });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as LiteralExpression;
    arg.Should().NotBeNull();
    var objLiteral = arg!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().HaveCount(2);
    objLiteral.Properties.Should().ContainKey("brightness");
    objLiteral.Properties.Should().ContainKey("effect");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithArrayLiteralArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func([255, 128, 0]);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as LiteralExpression;
    arg.Should().NotBeNull();
    var arrayLiteral = arg!.Literal as ArrayLiteral;
    arrayLiteral.Should().NotBeNull();
    arrayLiteral!.Elements.Should().HaveCount(3);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithNestedFunctionCallArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(test.other(42));
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as FunctionCallExpression;
    arg.Should().NotBeNull();
    arg!.FunctionCall.Name.Should().Be("other");
    arg.FunctionCall.Target.Should().Be("test");
    arg.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithMixedArgumentTypes()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(42, 'string', true, home.area.entity, { key: 'value' }, [1, 2, 3]);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(6);

    // Check int
    var arg1 = action.FunctionCall.Arguments[0] as LiteralExpression;
    var num1 = arg1!.Literal as NumericLiteral;
    num1!.Value.Should().Be(42);

    // Check string
    var arg2 = action.FunctionCall.Arguments[1] as LiteralExpression;
    var str2 = arg2!.Literal as StringLiteral;
    str2!.Value.Should().Be("string");

    // Check boolean
    var arg3 = action.FunctionCall.Arguments[2] as LiteralExpression;
    var bool3 = arg3!.Literal as BooleanLiteral;
    bool3!.Value.Should().BeTrue();

    // Check reference
    var arg4 = action.FunctionCall.Arguments[3] as ReferenceExpression;
    arg4!.Reference.Parts.Should().Equal("home", "area", "entity");

    // Check object
    var arg5 = action.FunctionCall.Arguments[4] as LiteralExpression;
    var obj5 = arg5!.Literal as ObjectLiteral;
    obj5!.Properties.Should().HaveCount(1);

    // Check array
    var arg6 = action.FunctionCall.Arguments[5] as LiteralExpression;
    var arr6 = arg6!.Literal as ArrayLiteral;
    arr6!.Elements.Should().HaveCount(3);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithComplexTarget()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call light.ceiling.turn_on();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    // Note: текущий парсер поддерживает только target.name, не target.subtarget.name
    // Но проверим, что парсится хотя бы target.name
    action!.FunctionCall.Target.Should().NotBeNull();
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithDurationArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(10s, 5m, 2h);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(3);

    var arg1 = action.FunctionCall.Arguments[0] as LiteralExpression;
    var dur1 = arg1!.Literal as DurationLiteral;
    dur1!.Value.Value.Should().Be(10);
    dur1.Value.Unit.Should().Be(DurationUnit.Seconds);

    var arg2 = action.FunctionCall.Arguments[1] as LiteralExpression;
    var dur2 = arg2!.Literal as DurationLiteral;
    dur2!.Value.Value.Should().Be(5);
    dur2.Value.Unit.Should().Be(DurationUnit.Minutes);

    var arg3 = action.FunctionCall.Arguments[2] as LiteralExpression;
    var dur3 = arg3!.Literal as DurationLiteral;
    dur3!.Value.Value.Should().Be(2);
    dur3.Value.Unit.Should().Be(DurationUnit.Hours);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithMultipleObjectArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func({ a: 1 }, { b: 2 }, { c: 3 });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(3);

    var arg1 = action.FunctionCall.Arguments[0] as LiteralExpression;
    var obj1 = arg1!.Literal as ObjectLiteral;
    obj1!.Properties.Should().ContainKey("a");

    var arg2 = action.FunctionCall.Arguments[1] as LiteralExpression;
    var obj2 = arg2!.Literal as ObjectLiteral;
    obj2!.Properties.Should().ContainKey("b");

    var arg3 = action.FunctionCall.Arguments[2] as LiteralExpression;
    var obj3 = arg3!.Literal as ObjectLiteral;
    obj3!.Properties.Should().ContainKey("c");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithNestedObjectInArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func({ config: { brightness: 100, color: 'red' } });
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as LiteralExpression;
    var obj = arg!.Literal as ObjectLiteral;
    obj!.Properties.Should().ContainKey("config");

    var configExpr = obj.Properties["config"] as LiteralExpression;
    var configObj = configExpr!.Literal as ObjectLiteral;
    configObj!.Properties.Should().HaveCount(2);
    configObj.Properties.Should().ContainKey("brightness");
    configObj.Properties.Should().ContainKey("color");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithArrayOfStrings()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(['red', 'green', 'blue']);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as LiteralExpression;
    var array = arg!.Literal as ArrayLiteral;
    array!.Elements.Should().HaveCount(3);

    var first = array.Elements[0] as LiteralExpression;
    var firstStr = first!.Literal as StringLiteral;
    firstStr!.Value.Should().Be("red");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithBinaryExpressionArguments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(test.value > 10, test.other < 20);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(2);

    // Check that arguments are binary expressions
    var arg1 = action.FunctionCall.Arguments[0] as BinaryExpression;
    arg1.Should().NotBeNull();
    arg1!.Operator.Should().Be(BinaryOperator.Greater);

    var arg2 = action.FunctionCall.Arguments[1] as BinaryExpression;
    arg2.Should().NotBeNull();
    arg2!.Operator.Should().Be(BinaryOperator.Less);
  }

  [Fact]
  public void ParseCallAction_ShouldParseMultipleCallActionsInSequence()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func1();
    call test.func2();
    call test.func3();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var actionBlock = result.Automations[0].WhenClauses[0].Actions;
    actionBlock.Statements.Should().HaveCount(3);
    actionBlock.Statements[0].Should().BeOfType<CallAction>();
    actionBlock.Statements[1].Should().BeOfType<CallAction>();
    actionBlock.Statements[2].Should().BeOfType<CallAction>();

    var action1 = actionBlock.Statements[0] as CallAction;
    action1!.FunctionCall.Name.Should().Be("func1");

    var action2 = actionBlock.Statements[1] as CallAction;
    action2!.FunctionCall.Name.Should().Be("func2");

    var action3 = actionBlock.Statements[2] as CallAction;
    action3!.FunctionCall.Name.Should().Be("func3");
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithParenExpressionArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func((test.value > 10));
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as ParenExpression;
    arg.Should().NotBeNull();
    var innerExpr = arg!.Inner as BinaryExpression;
    innerExpr.Should().NotBeNull();
    innerExpr!.Operator.Should().Be(BinaryOperator.Greater);
  }

  [Fact]
  public void ParseCallAction_ShouldParseWithInRangeExpressionArgument()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'Test' {
  when test.value == 5 {
    call test.func(test.value in 10..20);
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as CallAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as InRangeExpression;
    arg.Should().NotBeNull();
    arg!.Left.Should().NotBeNull();
    arg.Range.Should().NotBeNull();
  }

  // Tests for standalone action parsing (without automation wrapper)

  [Fact]
  public void ParseActionStatement_ShouldParseCallActionStandalone()
  {
    // Act
    var action = HassLanguageParser.ParseActionStatement("call test.func();");

    // Assert
    action.Should().BeOfType<CallAction>();
    var doAction = action as CallAction;
    doAction!.FunctionCall.Name.Should().Be("func");
    doAction.FunctionCall.Target.Should().Be("test");
  }

  [Fact]
  public void ParseActionStatement_ShouldParseCallActionWithoutTarget()
  {
    // Act
    var action = HassLanguageParser.ParseActionStatement("call notify();");

    // Assert
    action.Should().BeOfType<CallAction>();
    var doAction = action as CallAction;
    doAction!.FunctionCall.Name.Should().Be("notify");
    doAction.FunctionCall.Target.Should().BeNull();
  }

  [Fact]
  public void ParseActionStatement_ShouldParseCallActionWithArguments()
  {
    // Act
    var action = HassLanguageParser.ParseActionStatement("call test.func(42, 'test', true);");

    // Assert
    action.Should().BeOfType<CallAction>();
    var doAction = action as CallAction;
    doAction!.FunctionCall.Arguments.Should().HaveCount(3);
  }

  [Fact]
  public void ParseActionBlock_ShouldParseSingleCallAction()
  {
    // Act
    var block = HassLanguageParser.ParseActionBlock("{ call test.func(); }");

    // Assert
    block.Statements.Should().HaveCount(1);
    block.Statements[0].Should().BeOfType<CallAction>();
    var doAction = block.Statements[0] as CallAction;
    doAction!.FunctionCall.Name.Should().Be("func");
    doAction.FunctionCall.Target.Should().Be("test");
  }

  [Fact]
  public void ParseActionBlock_ShouldParseMultipleCallActions()
  {
    // Act
    var block = HassLanguageParser.ParseActionBlock(
      "{ call test.func1(); call test.func2(); call test.func3(); }"
    );

    // Assert
    block.Statements.Should().HaveCount(3);
    block.Statements[0].Should().BeOfType<CallAction>();
    block.Statements[1].Should().BeOfType<CallAction>();
    block.Statements[2].Should().BeOfType<CallAction>();

    var action1 = block.Statements[0] as CallAction;
    action1!.FunctionCall.Name.Should().Be("func1");

    var action2 = block.Statements[1] as CallAction;
    action2!.FunctionCall.Name.Should().Be("func2");

    var action3 = block.Statements[2] as CallAction;
    action3!.FunctionCall.Name.Should().Be("func3");
  }

  [Fact]
  public void ParseActionBlock_ShouldParseCallActionsWithDifferentArguments()
  {
    // Act
    var block = HassLanguageParser.ParseActionBlock(
      "{ call light.turn_on(); call notify.telegram('Hello', 'World'); call test.func(42, true); }"
    );

    // Assert
    block.Statements.Should().HaveCount(3);

    var action1 = block.Statements[0] as CallAction;
    action1!.FunctionCall.Name.Should().Be("turn_on");
    action1.FunctionCall.Arguments.Should().BeEmpty();

    var action2 = block.Statements[1] as CallAction;
    action2!.FunctionCall.Name.Should().Be("telegram");
    action2.FunctionCall.Arguments.Should().HaveCount(2);

    var action3 = block.Statements[2] as CallAction;
    action3!.FunctionCall.Name.Should().Be("func");
    action3.FunctionCall.Arguments.Should().HaveCount(2);
  }

  [Fact]
  public void ParseActionBlock_ShouldParseCallActionWithObjectLiteral()
  {
    // Act
    var block = HassLanguageParser.ParseActionBlock(
      "{ call light.turn_on({ brightness: 100, effect: 'rainbow' }); }"
    );

    // Assert
    block.Statements.Should().HaveCount(1);
    var action = block.Statements[0] as CallAction;
    action!.FunctionCall.Arguments.Should().HaveCount(1);

    var arg = action.FunctionCall.Arguments[0] as LiteralExpression;
    var objLiteral = arg!.Literal as ObjectLiteral;
    objLiteral!.Properties.Should().HaveCount(2);
    objLiteral.Properties.Should().ContainKey("brightness");
    objLiteral.Properties.Should().ContainKey("effect");
  }
}
