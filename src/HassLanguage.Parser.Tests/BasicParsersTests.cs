using FluentAssertions;
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
    var result = HassLanguageParser.Parse($"home 'Test' {expected} {{ }}");

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be(expected);
  }

  [Theory]
  [InlineData("\"simple\"", "simple")]
  [InlineData("\"with spaces\"", "with spaces")]
  [InlineData("\"\"", "")]
  [InlineData("'simple'", "simple")]
  [InlineData("'with spaces'", "with spaces")]
  [InlineData("''", "")]
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
      $"automation 'Test' {{ when {input} == {expected} {{ do test(); }} }}"
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
      $"automation 'Test' {{ when {input} > 0.0 {{ do test(); }} }}"
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
      $"automation 'Test' {{ when {input} == {expected.ToString().ToLower()} {{ do test(); }} }}"
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
home 'Test' test {
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
home 'Test' test {
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
    var input = "home   'Test'   test   {   }";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreLineCommentAtEndOfLine()
  {
    // Arrange
    var input = "home 'Test' test { } // comment at end";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreLineCommentBetweenTokens()
  {
    // Arrange
    var input = "home // comment\n'Test' test { }";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].DisplayName.Should().Be("Test");
  }

  [Fact]
  public void Parse_ShouldIgnoreMultiLineBlockComment()
  {
    // Arrange
    var input =
      @"
/* This is a
   multi-line
   block comment */
home 'Test' test {
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreCommentsInExpressions()
  {
    // Arrange
    var input =
      @"
automation 'Test' {
  when test.value == 5 {
    do test.func(42);
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as Core.Ast.DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
  }

  [Fact]
  public void Parse_ShouldIgnoreCommentsInObjectLiteral()
  {
    // Arrange
    var input =
      @"
automation 'Test' {
  when test.value == 5 {
    do test.func({
      brightness: 100,
      effect: 'rainbow'
    });
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as Core.Ast.DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var firstArg = action.FunctionCall.Arguments[0] as Core.Ast.LiteralExpression;
    firstArg.Should().NotBeNull();
    var objLiteral = firstArg!.Literal as Core.Ast.ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().HaveCount(2);
    objLiteral.Properties.Should().ContainKey("brightness");
    objLiteral.Properties.Should().ContainKey("effect");
  }

  [Fact]
  public void Parse_ShouldIgnoreCommentsInArrayLiteral()
  {
    // Arrange
    var input =
      @"
automation 'Test' {
  when test.value == 5 {
    do test.func({
      rgb_color: [255, 0, 0]
    });
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as Core.Ast.DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var firstArg = action.FunctionCall.Arguments[0] as Core.Ast.LiteralExpression;
    firstArg.Should().NotBeNull();
    var objLiteral = firstArg!.Literal as Core.Ast.ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().ContainKey("rgb_color");
    var rgbExpr = objLiteral.Properties["rgb_color"] as Core.Ast.LiteralExpression;
    rgbExpr.Should().NotBeNull();
    var rgbArray = rgbExpr!.Literal as Core.Ast.ArrayLiteral;
    rgbArray.Should().NotBeNull();
    rgbArray!.Elements.Should().HaveCount(3);
  }

  [Fact]
  public void Parse_ShouldIgnoreCommentsInComplexStructure()
  {
    // Arrange
    var input =
      @"
// Comment before home
home 'MyHome' {
  // Comment before room
  room ""Living Room"" living {
    // Comment before device
    device 'Light' light {
      entities: [
        // Comment before entity
        light chandelier { id: 'light.chandelier'; }
        // Comment after entity
      ];
    }
  }
}

// Comment before automation
automation 'Test' {
  when living.light.chandelier == 'on' {
    do light.turn_on(living.light.chandelier);
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Rooms.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Devices.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Devices[0].Entities.Should().HaveCount(1);
    result.Automations.Should().HaveCount(1);
  }

  [Fact]
  public void Parse_ShouldIgnoreEmptyLineComment()
  {
    // Arrange
    var input = "home 'Test' test { } //";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreEmptyBlockComment()
  {
    // Arrange
    var input = "home 'Test' test { } /**/";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldIgnoreCommentsWithSpecialCharacters()
  {
    // Arrange
    var input =
      @"
// Comment with special chars: !@#$%^&*()_+-=[]{}|;:'"",.<>?
home 'Test' test {
  /* Block comment with 'quotes' and 'apostrophes' */
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Alias.Should().Be("test");
  }

  [Fact]
  public void Parse_ShouldParseSingleQuotesInAutomation()
  {
    // Arrange
    var input =
      @"
automation 'Test Automation' {
  when test.value == 'on' {
    do notify.telegram('Message with single quotes');
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].DisplayName.Should().Be("Test Automation");
  }

  [Fact]
  public void Parse_ShouldParseMixedQuotes()
  {
    // Arrange
    var input =
      @"
home 'My Home' {
  room ""Living Room"" living {
    device 'Light Device' light {
      entities: [
        light main { id: 'light.main'; }
      ];
    }
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].DisplayName.Should().Be("My Home");
    result.Homes[0].Rooms.Should().HaveCount(1);
    result.Homes[0].Rooms[0].DisplayName.Should().Be("Living Room");
    result.Homes[0].Rooms[0].Devices.Should().HaveCount(1);
    result.Homes[0].Rooms[0].Devices[0].DisplayName.Should().Be("Light Device");
    result.Homes[0].Rooms[0].Devices[0].Entities.Should().HaveCount(1);
  }

  [Fact]
  public void Parse_ShouldParseSingleQuotesInExpressions()
  {
    // Arrange
    var input =
      @"
automation 'Test' {
  when test.value == 'on' {
    do test.func('string with single quotes');
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as Core.Ast.DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var arg = action.FunctionCall.Arguments[0] as Core.Ast.LiteralExpression;
    var strLiteral = arg!.Literal as Core.Ast.StringLiteral;
    strLiteral!.Value.Should().Be("string with single quotes");
  }

  [Fact]
  public void Parse_ShouldParseSingleQuotesInObjectLiteral()
  {
    // Arrange
    var input =
      @"
automation 'Test' {
  when test.value == 5 {
    do test.func({
      name: 'test name',
      effect: 'rainbow'
    });
  }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    var action = result.Automations[0].WhenClauses[0].Actions.Statements[0] as Core.Ast.DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Arguments.Should().HaveCount(1);
    var firstArg = action.FunctionCall.Arguments[0] as Core.Ast.LiteralExpression;
    var objLiteral = firstArg!.Literal as Core.Ast.ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().HaveCount(2);

    var nameExpr = objLiteral.Properties["name"] as Core.Ast.LiteralExpression;
    var nameStr = nameExpr!.Literal as Core.Ast.StringLiteral;
    nameStr!.Value.Should().Be("test name");

    var effectExpr = objLiteral.Properties["effect"] as Core.Ast.LiteralExpression;
    var effectStr = effectExpr!.Literal as Core.Ast.StringLiteral;
    effectStr!.Value.Should().Be("rainbow");
  }
}
