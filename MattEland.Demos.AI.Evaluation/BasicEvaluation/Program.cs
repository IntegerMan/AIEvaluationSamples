using BasicEvaluation;

#pragma warning disable AIEVAL001

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

// Connect to OpenAI
OpenAIClientOptions options = new()
{
	Endpoint = new Uri(settings.OpenAIEndpoint)
};
ApiKeyCredential key = new ApiKeyCredential(settings.OpenAIKey);
IChatClient chatClient = new OpenAIClient(key, options)
		.GetChatClient(settings.TextModelName)
		.AsIChatClient();

// Build our chat history
const string greeting = "How can I help you today?";
console.MarkupLineInterpolated($"[cyan]AI[/]: {greeting}");

string userText = "Is today after May 1st? If so, tell me what the next month will be.";
console.MarkupLineInterpolated($"[yellow]User[/]: {userText}");

string ragContext = "The current date is May 27th";

List<ChatMessage> messages = [
	new(ChatRole.System, $"{settings.SystemPrompt} {ragContext}"),
	new(ChatRole.Assistant, greeting),
	new(ChatRole.User, userText)
];

// Get a response from the LLM
ChatResponse responses = await chatClient.GetResponseAsync(messages);
foreach (var response in responses.Messages)
{
	console.MarkupLineInterpolated($"[cyan]AI[/]: {response.Text}");
}

// Set up evaluators
IEvaluator evaluator = new CompositeEvaluator(
	new CoherenceEvaluator(),
	new CompletenessEvaluator(),
	new FluencyEvaluator(),
	new GroundednessEvaluator(),
	new RelevanceEvaluator(),
	new RelevanceTruthAndCompletenessEvaluator(),
	new EquivalenceEvaluator(),
	new RetrievalEvaluator()
);

List<EvaluationContext> context = [
	new RetrievalEvaluatorContext(ragContext),
	new CompletenessEvaluatorContext("Today is May 27th and the next month is June"),
	new EquivalenceEvaluatorContext("The current date is May 27th, which is after May 1st and before June."),
	new GroundednessEvaluatorContext("May 27th is after May 1st. June is the month immediately following May.")
];

// Evaluate the response
ChatConfiguration chatConfig = new(chatClient);
EvaluationResult evalResult = await evaluator.EvaluateAsync(messages, responses, chatConfig, context);
console.DisplayEvaluationResultsTable(evalResult);
