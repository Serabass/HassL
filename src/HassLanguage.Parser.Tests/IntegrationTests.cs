using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class IntegrationTests
{
  [Fact]
  public void Parse_ShouldParseBasicExample()
  {
    // Arrange
    var input =
      @"
home 'TestHome' {
  settings { }

  area 'TestRoom' test {
    device 'TestDevice' test {
      entities: [
        binary_sensor test_sensor = 'binary_sensor.test'
      ];
    }
  }
}

automation 'Simple test' {
  when test.test.test_sensor == 'on' {
    do notify.telegram('Sensor activated!');
  }
}";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].DisplayName.Should().Be("TestHome");
    result.Homes[0].Areas.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].Entities.Should().HaveCount(1);
    result.Automations.Should().HaveCount(1);
    result.Automations[0].DisplayName.Should().Be("Simple test");
  }

  [Fact]
  public void Parse_ShouldParseComplexExample()
  {
    // Arrange
    var input =
      @"
home 'MyFlat' {
  area 'Kitchen' kitchen {
    device 'Light' light {
      entities: [
        light ceiling = 'light.kitchen_ceiling',
        light counter = 'light.kitchen_counter'
      ];
    }

    device 'Sensors' sensors {
      entities: [
        binary_sensor motion = 'binary_sensor.kitchen_motion',
        sensor temp = 'sensor.kitchen_temperature'; unit: '°C'
      ];
    }
  }

  area 'Living Room' living {
    device 'Climate' climate {
      entities: [
        climate ac = 'climate.living_ac'
      ];
    }

    device 'Sensors' sensors {
      entities: [
        sensor temp = 'sensor.living_temperature'; unit: '°C',
        sensor humidity = 'sensor.living_humidity'; unit: '%'
      ];
    }
  }
}

automation 'Kitchen motion light' {
  when sensors.motion == 'on' {
    do light.turn_on(light.ceiling, { brightness: 70 });
    wait sensors.motion == 'off' for 40s timeout 10m;
    do light.turn_off(light.ceiling);
  }
}

automation 'AC auto cool' {
  when living.climate.temp > 25.0 for 30m {
    do climate.set_mode(living.climate.ac, 'cool');
  }
}

automation 'Extreme conditions alert' {
  when any {
    living.sensors.temp > 28.0;
    living.sensors.humidity > 70;
  } {
    do notify.telegram('Alert!');
  }
}";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas.Should().HaveCount(2);
    result.Automations.Should().HaveCount(3);

    // Check first automation
    var firstAutomation = result.Automations[0];
    firstAutomation.DisplayName.Should().Be("Kitchen motion light");
    firstAutomation.WhenClauses.Should().HaveCount(1);
    firstAutomation.WhenClauses[0].Actions.Statements.Should().HaveCount(3);

    // Check second automation
    var secondAutomation = result.Automations[1];
    secondAutomation.DisplayName.Should().Be("AC auto cool");
    var condition = secondAutomation.WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    condition!.ForDuration.Should().NotBeNull();
    condition.ForDuration!.Value.Should().Be(30);
    condition.ForDuration.Unit.Should().Be(Core.Ast.DurationUnit.Minutes);

    // Check third automation
    var thirdAutomation = result.Automations[2];
    thirdAutomation.DisplayName.Should().Be("Extreme conditions alert");
    var anyCondition = thirdAutomation.WhenClauses[0].Condition as Core.Ast.AnyCondition;
    anyCondition.Should().NotBeNull();
    anyCondition!.Conditions.Should().HaveCount(2);
  }

  [Fact]
  public void Parse_ShouldParseWithDecorators()
  {
    // Arrange
    var input =
      @"
@mode(restart)
@cooldown(10s)
automation 'Bathroom light' {
  @edge(rising)
  @debounce(2s)
  when bathroom.sensors.motion == 'on' {
    do light.turn_on(bathroom.light.main, { brightness: 100 });
  }
}";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].Decorators.Should().HaveCount(2);
    result.Automations[0].WhenClauses[0].Decorators.Should().HaveCount(2);
  }

  [Fact]
  public void Parse_ShouldParseMultipleHomes()
  {
    // Arrange
    var input =
      @"
home 'Home1' home1 { }
home 'Home2' home2 { }
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(2);
    result.Homes[0].Alias.Should().Be("home1");
    result.Homes[1].Alias.Should().Be("home2");
  }

  [Fact]
  public void Parse_ShouldParseMultipleAutomations()
  {
    // Arrange
    var input =
      @"
automation 'Auto1' {
  when test.value == 1 { do test1(); }
}

automation 'Auto2' {
  when test.value == 2 { do test2(); }
}

automation 'Auto3' {
  when test.value == 3 { do test3(); }
}
";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Automations.Should().HaveCount(3);
    result.Automations[0].DisplayName.Should().Be("Auto1");
    result.Automations[1].DisplayName.Should().Be("Auto2");
    result.Automations[2].DisplayName.Should().Be("Auto3");
  }

  [Fact]
  public void Parse_ShouldHandleEmptyProgram()
  {
    // Act
    var result = HassLanguageParser.Parse("");

    // Assert
    result.Homes.Should().BeEmpty();
    result.Automations.Should().BeEmpty();
  }

  [Fact]
  public void Parse_ShouldThrowOnInvalidSyntax()
  {
    // Arrange
    var input = "home 'Test' test { invalid syntax }";

    // Act & Assert
    var act = () => HassLanguageParser.Parse(input);
    act.Should().Throw<ParseException>();
  }

  [Fact]
  public void Parse_ShouldParseComplexNestedStructures()
  {
    // Arrange
    var input =
      @"
home 'Complex' complex {
  area 'Area1' area1 {
    device 'Device1' device1 {
      entities: [
        light main = 'light.main',
        sensor temp = 'sensor.temp'; unit: '°C'
      ];
    }
  }
  
  area 'Area2' area2 {
    device 'Device2' device2 {
      entities: [
        switch sw = 'switch.sw'
      ];
    }
  }
}

automation 'Complex Auto' {
  when all {
    area1.device1.temp > 25.0;
    area2.device2.sw == 'on';
  } for 5m {
    do notify.telegram('Complex condition met');
    wait area1.device1.temp < 20.0 for 10m timeout 1h;
    do notify.telegram('Temperature dropped');
  }
}";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas.Should().HaveCount(2);
    result.Automations.Should().HaveCount(1);
    var automation = result.Automations[0];
    automation.WhenClauses.Should().HaveCount(1);
    var allCondition = automation.WhenClauses[0].Condition as Core.Ast.AllCondition;
    allCondition.Should().NotBeNull();
    allCondition!.Conditions.Should().HaveCount(2);
    automation.WhenClauses[0].Actions.Statements.Should().HaveCount(3);
  }

  [Fact]
  public void Parse_ShouldParseVoiceCommandToRainbowChandelier()
  {
    // Arrange
    var input =
      @"
home 'MyHome' {
  area 'Living Room' living {
    device 'Light' light {
      entities: [
        light chandelier = 'light.living_chandelier'
      ];
    }

    device 'Voice' voice {
      entities: [
        sensor command = 'sensor.voice_command'
      ];
    }
  }
}

automation 'Update light to rainbow' {
  when voice.command == 'обнови свет' {
    do light.turn_on(light.chandelier, { brightness: 100, effect: 'rainbow' });
  }
}";

    // Act
    var result = HassLanguageParser.Parse(input);

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas.Should().HaveCount(1);
    var area = result.Homes[0].Areas[0];
    area.Devices.Should().HaveCount(2);

    // Check light device
    var lightDevice = area.Devices.First(d => d.Alias == "light");
    lightDevice.Entities.Should().HaveCount(1);
    lightDevice.Entities[0].Alias.Should().Be("chandelier");

    // Check voice device
    var voiceDevice = area.Devices.First(d => d.Alias == "voice");
    voiceDevice.Entities.Should().HaveCount(1);
    voiceDevice.Entities[0].Alias.Should().Be("command");

    // Check automation
    result.Automations.Should().HaveCount(1);
    var automation = result.Automations[0];
    automation.DisplayName.Should().Be("Update light to rainbow");
    automation.WhenClauses.Should().HaveCount(1);

    // Check condition
    var condition = automation.WhenClauses[0].Condition as SingleCondition;
    condition.Should().NotBeNull();
    var expr = condition!.Expression as BinaryExpression;
    expr.Should().NotBeNull();
    expr!.Operator.Should().Be(BinaryOperator.Equals);

    // Check action
    automation.WhenClauses[0].Actions.Statements.Should().HaveCount(1);
    var action = automation.WhenClauses[0].Actions.Statements[0] as DoAction;
    action.Should().NotBeNull();
    action!.FunctionCall.Target.Should().Be("light");
    action.FunctionCall.Name.Should().Be("turn_on");
    action.FunctionCall.Arguments.Should().HaveCount(2);

    // Check that second argument is object literal with rainbow effect
    var secondArg = action.FunctionCall.Arguments[1] as LiteralExpression;
    secondArg.Should().NotBeNull();
    var objLiteral = secondArg!.Literal as ObjectLiteral;
    objLiteral.Should().NotBeNull();
    objLiteral!.Properties.Should().ContainKey("brightness");
    objLiteral.Properties.Should().ContainKey("effect");

    // Check effect value is "rainbow"
    var effectExpr = objLiteral.Properties["effect"] as LiteralExpression;
    effectExpr.Should().NotBeNull();
    var effectLiteral = effectExpr!.Literal as StringLiteral;
    effectLiteral.Should().NotBeNull();
    effectLiteral!.Value.Should().Be("rainbow");
  }
}
