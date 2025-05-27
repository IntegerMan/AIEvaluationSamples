using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using Spectre.Console;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

// Everything is better with a nice header
IAnsiConsole console = AnsiConsole.Console;
console.Write(new FigletText("AI Evaluation").Color(Color.Yellow));
console.MarkupLine("[cyan]Showcasing Microsoft.Extensions.AI.Evaluation[/]");
console.WriteLine();

// Load Settings
IConfiguration config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.AddCommandLine(args)
	.Build();
EvaluationDemoSettings settings = config.Get<EvaluationDemoSettings>()!;

const string greeting = "How can I help you today?";

console.MarkupLineInterpolated($"[cyan]AI[/]: {greeting}");
string userText = console.Ask<string>("[yellow]User[/]: ");


OpenAIClientOptions options = new OpenAIClientOptions();
options.Endpoint = new Uri(settings.OpenAIEndpoint);
ApiKeyCredential key = new ApiKeyCredential(settings.OpenAIKey);

IChatClient chatClient = new OpenAIClient(key, options)
		.GetChatClient(settings.TextModelName)
		.AsIChatClient();

List<ChatMessage> messages = [
	new(ChatRole.System, settings.SystemPrompt),
	new(ChatRole.Assistant, greeting),
	new(ChatRole.User, userText)
];
ChatResponse responses = await chatClient.GetResponseAsync(messages);

foreach (var response in responses.Messages)
{
	console.MarkupLineInterpolated($"[cyan]AI[/]: {response.Text}");
//	messages.Add(response);
}

ChatConfiguration chatConfig = new(chatClient);
IEvaluator evaluator = new CoherenceEvaluator();
EvaluationResult evalResult = await evaluator.EvaluateAsync(messages, responses, chatConfig);

console.WriteLine();
console.MarkupLine("[bold]Evaluation Results:[/]");
foreach (var metric in evalResult.Metrics)
{
	console.MarkupLineInterpolated($"[green]{metric.Key}[/]: {metric.Value}");
}