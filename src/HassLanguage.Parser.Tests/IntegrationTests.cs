using FluentAssertions;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class IntegrationTests
{
    [Fact]
    public void Parse_ShouldParseBasicExample()
    {
        // Arrange
        var input = @"
home ""TestHome"" {
  settings { }

  room ""TestRoom"" test {
    device ""TestDevice"" test {
      entities: [
        binary_sensor test_sensor { id: ""binary_sensor.test""; }
      ];
    }
  }
}

automation ""Simple test"" {
  when test.test.test_sensor == ""on"" {
    do notify.telegram(""Sensor activated!"");
  }
}";

        // Act
        var result = HassLanguageParser.Parse(input);

        // Assert
        result.Homes.Should().HaveCount(1);
        result.Homes[0].DisplayName.Should().Be("TestHome");
        result.Homes[0].Rooms.Should().HaveCount(1);
        result.Homes[0].Rooms[0].Devices.Should().HaveCount(1);
        result.Homes[0].Rooms[0].Devices[0].Entities.Should().HaveCount(1);
        result.Automations.Should().HaveCount(1);
        result.Automations[0].DisplayName.Should().Be("Simple test");
    }

    [Fact]
    public void Parse_ShouldParseComplexExample()
    {
        // Arrange
        var input = @"
home ""MyFlat"" {
  room ""Kitchen"" kitchen {
    device ""Light"" light {
      entities: [
        light ceiling { id: ""light.kitchen_ceiling""; },
        light counter { id: ""light.kitchen_counter""; }
      ];
    }

    device ""Sensors"" sensors {
      entities: [
        binary_sensor motion { id: ""binary_sensor.kitchen_motion""; },
        sensor temp { id: ""sensor.kitchen_temperature""; unit: ""°C""; }
      ];
    }
  }

  room ""Living Room"" living {
    device ""Climate"" climate {
      entities: [
        climate ac { id: ""climate.living_ac""; }
      ];
    }

    device ""Sensors"" sensors {
      entities: [
        sensor temp { id: ""sensor.living_temperature""; unit: ""°C""; },
        sensor humidity { id: ""sensor.living_humidity""; unit: ""%""; }
      ];
    }
  }
}

automation ""Kitchen motion light"" {
  when sensors.motion == ""on"" {
    do light.turn_on(light.ceiling, { brightness: 70 });
    wait sensors.motion == ""off"" for 40s timeout 10m;
    do light.turn_off(light.ceiling);
  }
}

automation ""AC auto cool"" {
  when living.climate.temp > 25.0 for 30m {
    do climate.set_mode(living.climate.ac, ""cool"");
  }
}

automation ""Extreme conditions alert"" {
  when any {
    living.sensors.temp > 28.0;
    living.sensors.humidity > 70;
  } {
    do notify.telegram(""Alert!"");
  }
}";

        // Act
        var result = HassLanguageParser.Parse(input);

        // Assert
        result.Homes.Should().HaveCount(1);
        result.Homes[0].Rooms.Should().HaveCount(2);
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
        var input = @"
@mode(restart)
@cooldown(10s)
automation ""Bathroom light"" {
  @edge(rising)
  @debounce(2s)
  when bathroom.sensors.motion == ""on"" {
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
        var input = @"
home ""Home1"" home1 { }
home ""Home2"" home2 { }
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
        var input = @"
automation ""Auto1"" {
  when test.value == 1 { do test1(); }
}

automation ""Auto2"" {
  when test.value == 2 { do test2(); }
}

automation ""Auto3"" {
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
        var input = "home \"Test\" test { invalid syntax }";

        // Act & Assert
        var act = () => HassLanguageParser.Parse(input);
        act.Should().Throw<ParseException>();
    }

    [Fact]
    public void Parse_ShouldParseComplexNestedStructures()
    {
        // Arrange
        var input = @"
home ""Complex"" complex {
  room ""Room1"" room1 {
    device ""Device1"" device1 {
      entities: [
        light main { id: ""light.main""; },
        sensor temp { id: ""sensor.temp""; unit: ""°C""; }
      ];
    }
  }
  
  room ""Room2"" room2 {
    device ""Device2"" device2 {
      entities: [
        switch sw { id: ""switch.sw""; }
      ];
    }
  }
}

automation ""Complex Auto"" {
  when all {
    room1.device1.temp > 25.0;
    room2.device2.sw == ""on"";
  } for 5m {
    do notify.telegram(""Complex condition met"");
    wait room1.device1.temp < 20.0 for 10m timeout 1h;
    do notify.telegram(""Temperature dropped"");
  }
}";

        // Act
        var result = HassLanguageParser.Parse(input);

        // Assert
        result.Homes.Should().HaveCount(1);
        result.Homes[0].Rooms.Should().HaveCount(2);
        result.Automations.Should().HaveCount(1);
        var automation = result.Automations[0];
        automation.WhenClauses.Should().HaveCount(1);
        var allCondition = automation.WhenClauses[0].Condition as Core.Ast.AllCondition;
        allCondition.Should().NotBeNull();
        allCondition!.Conditions.Should().HaveCount(2);
        automation.WhenClauses[0].Actions.Statements.Should().HaveCount(3);
    }
}
