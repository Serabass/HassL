using HassLanguage.Parser;
var input = "automation "Test" { when test.value == 5 { do test.func(); } }";
try {
  var result = HassLanguageParser.Parse(input);
  Console.WriteLine("Automations count: {result.Automations.Count}");
  if (result.Automations.Count > 0) {
    Console.WriteLine("First automation: {result.Automations[0].DisplayName}");
  }
} catch (Exception ex) {
  Console.WriteLine("Error: {ex.Message}");
  if (ex.InnerException != null) {
    Console.WriteLine("Inner: {ex.InnerException.Message}");
  }
}
