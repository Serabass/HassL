using System.Linq;
using FluentAssertions;
using HassLanguage.Core.Ast;
using HassLanguage.Parser;
using Xunit;

namespace HassLanguage.Parser.Tests;

public class AreasTests
{
  [Fact]
  public void ParseArea_ShouldParseBasicArea()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
  }
}"
    );

    // Assert
    result.Zones.Should().HaveCount(1);
    result.Zones[0].Areas.Should().HaveCount(1);
    var area = result.Zones[0].Areas[0];
    area.DisplayName.Should().Be("Kitchen");
    area.Alias.Should().Be("kitchen");
    area.Type.Should().BeNull();
    area.Devices.Should().BeEmpty();
    area.Decorators.Should().BeEmpty();
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithType()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen kitchen_type {
  }
}"
    );

    // Assert
    result.Zones[0].Areas[0].Type.Should().Be("kitchen_type");
  }

  [Fact]
  public void ParseArea_ShouldParseMultipleAreas()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
  }
  area 'Living Room' living {
  }
  area 'Bedroom' bedroom {
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(3);
    result.Zones[0].Areas[0].Alias.Should().Be("kitchen");
    result.Zones[0].Areas[1].Alias.Should().Be("living");
    result.Zones[0].Areas[2].Alias.Should().Be("bedroom");
    result.Zones[0].Areas[0].DisplayName.Should().Be("Kitchen");
    result.Zones[0].Areas[1].DisplayName.Should().Be("Living Room");
    result.Zones[0].Areas[2].DisplayName.Should().Be("Bedroom");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithSingleDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
    device 'Light' light {
      entities: [
        light main = 'light.kitchen_main'
      ];
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Devices.Should().HaveCount(1);
    area.Devices[0].DisplayName.Should().Be("Light");
    area.Devices[0].Alias.Should().Be("light");
    area.Devices[0].Entities.Should().HaveCount(1);
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithMultipleDevices()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
    device 'Light' light {
      entities: [
        light main = 'light.kitchen_main'
      ];
    }
    device 'Sensors' sensors {
      entities: [
        binary_sensor motion = 'binary_sensor.kitchen_motion',
        sensor temp = 'sensor.kitchen_temp'
      ];
    }
    device 'Climate' climate {
      entities: [
        climate ac = 'climate.kitchen_ac'
      ];
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Devices.Should().HaveCount(3);
    area.Devices[0].Alias.Should().Be("light");
    area.Devices[1].Alias.Should().Be("sensors");
    area.Devices[2].Alias.Should().Be("climate");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithDecorators()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  @priority(high)
  @enabled(true)
  area 'Kitchen' kitchen {
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Decorators.Should().HaveCount(2);
    area.Decorators[0].Name.Should().Be("priority");
    area.Decorators[1].Name.Should().Be("enabled");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithComplexDecorators()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  @priority(high)
  @enabled(true)
  @mode('restart')
  area 'Kitchen' kitchen {
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Decorators.Should().HaveCount(3);
    area.Decorators[0].Name.Should().Be("priority");
    area.Decorators[1].Name.Should().Be("enabled");
    area.Decorators[2].Name.Should().Be("mode");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithTypeAndDecorators()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  @priority(high)
  area 'Kitchen' kitchen kitchen_type {
    device 'Light' light {
      entities: [
        light main = 'light.kitchen_main'
      ];
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Type.Should().Be("kitchen_type");
    area.Decorators.Should().HaveCount(1);
    area.Decorators[0].Name.Should().Be("priority");
    area.Devices.Should().HaveCount(1);
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithDifferentQuotes()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area ""Kitchen"" kitchen {
  }
  area 'Living Room' living {
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(2);
    result.Zones[0].Areas[0].DisplayName.Should().Be("Kitchen");
    result.Zones[0].Areas[1].DisplayName.Should().Be("Living Room");
  }

  [Fact]
  public void ParseArea_ShouldParseNestedStructure()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
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
        sensor temp = 'sensor.kitchen_temp',
        sensor humidity = 'sensor.kitchen_humidity'
      ];
    }
  }
  area 'Living Room' living {
    device 'Climate' climate {
      entities: [
        climate ac = 'climate.living_ac'
      ];
    }
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(2);

    var kitchen = result.Zones[0].Areas[0];
    kitchen.Alias.Should().Be("kitchen");
    kitchen.Devices.Should().HaveCount(2);
    kitchen.Devices[0].Entities.Should().HaveCount(2);
    kitchen.Devices[1].Entities.Should().HaveCount(3);

    var living = result.Zones[0].Areas[1];
    living.Alias.Should().Be("living");
    living.Devices.Should().HaveCount(1);
    living.Devices[0].Entities.Should().HaveCount(1);
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithEmptyDevice()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
    device 'Light' light {
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Devices.Should().HaveCount(1);
    area.Devices[0].Entities.Should().BeEmpty();
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithSpecialCharactersInName()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen & Dining' kitchen_dining {
  }
  area 'Living Room (Main)' living_main {
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(2);
    result.Zones[0].Areas[0].DisplayName.Should().Be("Kitchen & Dining");
    result.Zones[0].Areas[1].DisplayName.Should().Be("Living Room (Main)");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithComments()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  // Comment before area
  area 'Kitchen' kitchen {
    // Comment inside area
    device 'Light' light {
      entities: [
        // Comment before entity
        light main = 'light.kitchen_main'
      ];
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Alias.Should().Be("kitchen");
    area.Devices.Should().HaveCount(1);
    area.Devices[0].Entities.Should().HaveCount(1);
  }

  [Fact]
  public void ParseArea_ShouldHandleAreaWithoutAlias()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' {
  area 'Kitchen' kitchen {
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(1);
    result.Zones[0].Areas[0].Alias.Should().Be("kitchen");
  }

  [Fact]
  public void ParseArea_ShouldParseMultipleAreasWithSameType()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen 1' kitchen1 kitchen {
  }
  area 'Kitchen 2' kitchen2 kitchen {
  }
  area 'Bedroom 1' bedroom1 bedroom {
  }
  area 'Bedroom 2' bedroom2 bedroom {
  }
}"
    );

    // Assert
    result.Zones[0].Areas.Should().HaveCount(4);
    result.Zones[0].Areas[0].Type.Should().Be("kitchen");
    result.Zones[0].Areas[1].Type.Should().Be("kitchen");
    result.Zones[0].Areas[2].Type.Should().Be("bedroom");
    result.Zones[0].Areas[3].Type.Should().Be("bedroom");
  }

  [Fact]
  public void ParseArea_ShouldParseAreaWithMultipleEntities()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
    device 'Sensors' sensors {
      entities: [
        binary_sensor motion = 'binary_sensor.kitchen_motion',
        binary_sensor door = 'binary_sensor.kitchen_door',
        sensor temp = 'sensor.kitchen_temp'
      ];
    }
  }
}"
    );

    // Assert
    result.Zones.Should().HaveCount(1);
    var area = result.Zones[0].Areas[0];
    area.Devices.Should().HaveCount(1);
    var device = area.Devices[0];

    // Проверяем количество entities
    device.Entities.Should().HaveCount(3);

    // Проверяем типы и алиасы
    var entityTypes = device.Entities.Select(e => e.Type).ToList();
    var entityAliases = device.Entities.Select(e => e.Alias).ToList();

    entityTypes.Should().Contain("binary_sensor");
    entityTypes.Should().Contain("sensor");
    entityAliases.Should().Contain("motion");
    entityAliases.Should().Contain("door");
    entityAliases.Should().Contain("temp");
  }

  [Fact]
  public void ParseArea_ShouldParseEntityWithSimplifiedSyntaxWithEquals()
  {
    // Act
    var result = HassLanguageParser.Parse(
      @"zone 'TestHome' test {
  area 'Kitchen' kitchen {
    device 'Sensors' sensors {
      entities: [
        binary_sensor motion = 'binary_sensor.kitchen_motion',
        sensor temp = 'sensor.kitchen_temp'
      ];
    }
  }
}"
    );

    // Assert
    var area = result.Zones[0].Areas[0];
    area.Devices.Should().HaveCount(1);
    var device = area.Devices[0];
    device.Entities.Should().HaveCount(2);

    var motion = device.Entities.First(e => e.Alias == "motion");
    motion.Type.Should().Be("binary_sensor");
    motion.Properties.Should().ContainKey("id");

    var temp = device.Entities.First(e => e.Alias == "temp");
    temp.Type.Should().Be("sensor");
    temp.Properties.Should().ContainKey("id");
  }
}
