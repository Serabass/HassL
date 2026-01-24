using System.Text;
using HassLanguage.Runtime;

var runtime = new HassLanguageRuntime();

// Читаем пример из файла или используем встроенный
string exampleCode;

if (args.Length > 0 && File.Exists(args[0]))
{
  exampleCode = File.ReadAllText(args[0], Encoding.UTF8);
  Console.WriteLine($"Loading from file: {args[0]}");
}
else
{
  // Встроенный пример
  exampleCode =
    @"
home ""MyFlat"" {
  area ""Kitchen"" kitchen {
    device ""Light"" light {
      entities: [
        light ceiling { id: ""light.kitchen_ceiling""; },
        light counter { id: ""light.kitchen_counter""; }
      ];
    }

    device ""Sensors"" sensors {
      entities: [
        binary_sensor motion { id: ""binary_sensor.kitchen_motion""; },
        sensor illuminance { id: ""sensor.kitchen_lux""; unit: ""lx""; }
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
";
}

try
{
  Console.WriteLine("Parsing DSL code...");
  runtime.LoadProgram(exampleCode);
  Console.WriteLine("✓ Program parsed and validated successfully!");
  Console.WriteLine("\nAutomations registered:");
  // TODO: Add method to list registered automations
  Console.WriteLine("  - Ready to process events");
}
catch (Exception ex)
{
  Console.WriteLine($"✗ Error: {ex.Message}");
  if (ex.InnerException != null)
  {
    Console.WriteLine($"  Inner: {ex.InnerException.Message}");
  }
  Console.WriteLine("\nStack trace:");
  Console.WriteLine(ex.StackTrace);
  Environment.Exit(1);
}
