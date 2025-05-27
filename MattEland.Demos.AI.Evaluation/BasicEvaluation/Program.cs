using Microsoft.Extensions.Configuration;
using Spectre.Console;

// Everything is better with a nice header
IAnsiConsole console = AnsiConsole.Console;
console.Write(new FigletText("Kernel Memory").Color(Color.Yellow));
console.MarkupLine("[cyan]Kernel Memory Document Search Demo[/]");
console.WriteLine();

// Load Settings
IConfiguration config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.AddCommandLine(args)
	.Build();
EvaluationDemoSettings settings = config.Get<EvaluationDemoSettings>()!;