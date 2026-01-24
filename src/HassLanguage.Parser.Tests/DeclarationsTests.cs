using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class DeclarationsTests
{
  [Fact]
  public void ParseHomeDeclaration_ShouldParseBasicHome()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].DisplayName.Should().Be("TestHome");
    result.Homes[0].Alias.Should().Be("test");
    result.Homes[0].Areas.Should().BeEmpty();
  }

  [Fact]
  public void ParseHomeDeclaration_ShouldParseWithSettings()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  settings {
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Settings.Should().NotBeNull();
  }

  [Fact]
  public void ParseAreaDeclaration_ShouldParseBasicArea()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas.Should().HaveCount(1);
    result.Homes[0].Areas[0].DisplayName.Should().Be("TestArea");
    result.Homes[0].Areas[0].Alias.Should().Be("area");
  }

  [Fact]
  public void ParseAreaDeclaration_ShouldParseWithType()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area kitchen {
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Type.Should().Be("kitchen");
  }

  [Fact]
  public void ParseDeviceDeclaration_ShouldParseBasicDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    device 'TestDevice' device {
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].DisplayName.Should().Be("TestDevice");
    result.Homes[0].Areas[0].Devices[0].Alias.Should().Be("device");
  }

  [Fact]
  public void ParseDeviceDeclaration_ShouldParseWithType()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    device 'TestDevice' device light {
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    result.Homes[0].Areas[0].Devices[0].Type.Should().Be("light");
  }

  [Fact]
  public void ParseEntityDeclaration_ShouldParseBasicEntity()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    device 'TestDevice' device {
      entities: [
        light main = 'light.main'
      ];
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    var entities = result.Homes[0].Areas[0].Devices[0].Entities;
    entities.Should().HaveCount(1);
    entities[0].Type.Should().Be("light");
    entities[0].Alias.Should().Be("main");
    entities[0].Properties.Should().ContainKey("id");
  }

  [Fact]
  public void ParseEntityDeclaration_ShouldParseMultipleEntities()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"            home 'TestHome' test { 
            area 'TestArea' area { 
                device 'TestDevice' device { 
                    entities: [
                        light main = 'light.main',
                        sensor temp = 'sensor.temp'
                    ]; 
                } 
            } 
        }"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    var entities = result.Homes[0].Areas[0].Devices[0].Entities;
    entities.Should().HaveCount(2);
    entities[0].Type.Should().Be("light");
    entities[1].Type.Should().Be("sensor");
  }

  [Fact]
  public void ParseEntityDeclaration_ShouldParseEntityProperties()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"home 'TestHome' test {
  area 'TestArea' area {
    device 'TestDevice' device {
      entities: [
        sensor temp = 'sensor.temp'
      ];
    }
  }
}"
    );

    // Assert
    result.Homes.Should().HaveCount(1);
    var entity = result.Homes[0].Areas[0].Devices[0].Entities[0];
    entity.Properties.Should().HaveCount(1);
    entity.Properties.Should().ContainKey("id");
  }

  [Fact]
  public void ParseAutomationDeclaration_ShouldParseBasicAutomation()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation 'TestAutomation' {
  when test.value == 5 {
    do test();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].DisplayName.Should().Be("TestAutomation");
    result.Automations[0].WhenClauses.Should().HaveCount(1);
  }

  [Fact]
  public void ParseAutomationDeclaration_ShouldParseMultipleWhenClauses()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"automation ""TestAutomation"" {
  when test.value == 5 {
    do test1();
  }
  when test.value == 10 {
    do test2();
  }
}"
    );

    // Assert
    result.Automations.Should().HaveCount(1);
    result.Automations[0].WhenClauses.Should().HaveCount(2);
  }
}
