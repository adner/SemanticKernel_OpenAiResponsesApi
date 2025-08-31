using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI.Responses;
using System.Text.Json;
using Spectre.Console;
using Figgle;

// Display welcome banner
DisplayWelcomeBanner();

// Load configuration
var config = LoadConfig();

await CallResponseApi_OpenAI_WebSearch(config);

await CallResponsesApi_OpenAI(config);

await CallResponsesApi_Local();

// Display completion message
DisplayCompletionMessage();

static void DisplayWelcomeBanner()
{
    // ASCII Art Title using Figgle
    var title = FiggleFonts.Slant.Render("AI UNICORN GENERATOR");
    AnsiConsole.Write(new Markup($"[cyan1]{title.EscapeMarkup()}[/]"));
    
    // Welcome message with emojis and colors
    var welcomePanel = new Panel(new Markup("[bold yellow]This demo shows how Semantic Kernel can be used with the OpenAI Responses API to create short stories about unicorns.🦄🌈☀️[/]\n\n" +
                                          "[cyan] We do this in three different ways: [/]\n\n" +
                                          "[green]✨ Using the Web Search tool in the OpenAI Responses API to get info about unicorns.[/]\n" +
                                          "[blue]🤖 Use the OpenAI Responses API in the cloud to create a short story about a unicorn. [/]\n" +
                                          "[magenta]🐋 Use the experimental Responses API support of the Huggingface Transformers framework running locally, hosted in Docker. [/]\n\n" +
                                          "[dim]All powered by Semantic Kernel & OpenAI Responses API[/]"))
        .Border(BoxBorder.Double)
        .BorderColor(Color.Gold1)
        .Header("[bold white on blue] 🦄🦄🦄 UNICORN STORY CREATION USING SEMANTIC KERNEL 🦄🦄🦄 [/]");
    
    AnsiConsole.Write(welcomePanel);
    AnsiConsole.WriteLine();
    
    // Loading animation
    AnsiConsole.Status()
        .Start("Initializing systems...", ctx =>
        {
            ctx.Spinner(Spinner.Known.Star);
            ctx.SpinnerStyle(Style.Parse("green"));
            Thread.Sleep(2000);
        });
}

static void DisplayCompletionMessage()
{
    AnsiConsole.WriteLine();
    
    // Success message
    var completionPanel = new Panel(new Markup("[bold green]🎉 All demos completed successfully! 🎉[/]\n\n" +
                                              "[cyan]Thank you for watching the Semantic Kernel Responses API demo![/]\n" +
                                              "[yellow]✨ Hope you enjoyed the nice colors and emojis!✨[/]"))
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Green)
        .Header("[bold white on green] ✅ COMPLETION [/]");
    
    AnsiConsole.Write(completionPanel);
    
    // Farewell ASCII art
    var farewell = FiggleFonts.Small.Render("GOODBYE!");
    AnsiConsole.Write(new Markup($"[purple]{farewell.EscapeMarkup()}[/]"));
}

static async Task CallResponseApi_OpenAI_WebSearch(ConfigModel config)
{
    var panel = new Panel("[bold blue]🔍 Using the Semantic Kernel to use the Responses API to call OpenAI to do a web search about Unicorns...🔍[/]")
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Cyan1)
        .Header("[bold white on blue] 🌐 WEB SEARCH [/]");

    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();

    OpenAIResponseClient client = new OpenAIResponseClient("gpt-5-mini", new ApiKeyCredential(config.OpenAI.ApiKey));

    OpenAIResponseAgent agent = new(client)
    {
        StoreEnabled = false,
    };

    // ResponseCreationOptions allows you to specify tools for the agent.
    ResponseCreationOptions creationOptions = new();
    creationOptions.Tools.Add(ResponseTool.CreateWebSearchTool());
    OpenAIResponseAgentInvokeOptions invokeOptions = new()
    {
        ResponseCreationOptions = creationOptions,
    };

    // Invoke the agent and output the response
    var responseItems = agent.InvokeStreamingAsync("Do a web search for information about unicorns (the mythnical creature), summarize in three sentences.", options: invokeOptions);

    await foreach (var responseItem in responseItems)
    {
        AnsiConsole.Markup($"[bold cyan]{responseItem.Message.Content.EscapeMarkup()}[/]");
    }
    AnsiConsole.Markup("\n[grey]────────────────────────────────────────────────────────────[/]\n");

    
}

static async Task CallResponsesApi_Local()
{

    var panel = new Panel("[bold magenta]🐋 Using Semantic Kernel to create another story about unicorns using the experimental support for the Responses API in the Huggingface Transformers library, running locally in Docker...[/]")
        .Border(BoxBorder.Double)
        .BorderColor(Color.Magenta1)
        .Header("[bold white on magenta] 🏠 LOCAL OpenAI Responses API running in DOCKER [/]");

    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();

    var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
    var transport = new HttpClientPipelineTransport(httpClient);

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    OpenAIResponseClient client = new OpenAIResponseClient("openai/gpt-oss-20b", new ApiKeyCredential("NotRequiredForLocal!"), new OpenAI.OpenAIClientOptions()
    {
        Endpoint = new Uri("http://localhost:8000/v1"),
        Transport = transport
    });

    // Define the agent
    OpenAIResponseAgent agent = new(client)
    {
        Name = "ResponseAgent",
    };

    // Invoke the agent and output the response
    var responseItems = agent.InvokeStreamingAsync("Tell me a ten sentence bedtime story about a unicorn.");

    Console.WriteLine("\n");
    await foreach (var responseItem in responseItems)
    {
        AnsiConsole.Markup($"[bold cyan]{responseItem.Message.Content.EscapeMarkup()}[/]");
    }

    Console.WriteLine("\n"); 
    AnsiConsole.Markup("\n[grey]────────────────────────────────────────────────────────────[/]\n");
}

static async Task CallResponsesApi_OpenAI(ConfigModel config)
{
    Console.WriteLine("\n");

    var panel = new Panel("[bold green]🤖 Using the Responses API in OpenAI to create a story about unicorns...[/]")
        .Border(BoxBorder.Heavy)
        .BorderColor(Color.Green1)
        .Header("[bold white on green] ✨ OPENAI in the CLOUD! [/]");

    AnsiConsole.Write(panel);

    var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));
    var transport = new HttpClientPipelineTransport(httpClient);

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    OpenAIResponseClient client = new OpenAIResponseClient("gpt-5-mini", new ApiKeyCredential(config.OpenAI.ApiKey), new OpenAI.OpenAIClientOptions()
    {
        Transport = transport
    });

    // Define the agent
    OpenAIResponseAgent agent = new(client)
    {
        Name = "ResponseAgent",
    };

    // Invoke the agent and output the response
    var responseItems = agent.InvokeStreamingAsync("Tell me a ten sentence bedtime story about a unicorn.");

    Console.WriteLine("\n"); 
    await foreach (var responseItem in responseItems)
    {
        AnsiConsole.Markup($"[bold cyan]{responseItem.Message.Content.EscapeMarkup()}[/]");
    }

     AnsiConsole.Markup("\n[grey]────────────────────────────────────────────────────────────[/]\n");
}

static ConfigModel LoadConfig()
{
    try
    {
        var configPath = "config.json";
        if (!File.Exists(configPath))
        {
            AnsiConsole.MarkupLine($"[red]❌ Config file {configPath} not found. Please create it with your API keys.[/]");
            Environment.Exit(1);
        }

        var configJson = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<ConfigModel>(configJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.OpenAI?.ApiKey == null)
        {
            AnsiConsole.MarkupLine("[red]❌ OpenAI API key not found in config file.[/]");
            Environment.Exit(1);
        }

        return config;
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]❌ Error loading config: {ex.Message.EscapeMarkup()}[/]");
        Environment.Exit(1);
        return null; // Never reached
    }
} 

public sealed class LoggingHandler : DelegatingHandler
{
    public LoggingHandler(HttpMessageHandler inner) : base(inner) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        AnsiConsole.MarkupLine($"[green]➡️  {req.Method} [link]{req.RequestUri}[/][/]");
        foreach (var h in req.Headers)
        {
            if (h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine($"[dim]{h.Key}: [red][[MASKED]][/][/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[dim]{h.Key}: {string.Join(",", h.Value).EscapeMarkup()}[/]");
            }
        }
        if (req.Content != null) AnsiConsole.MarkupLine($"[dim]{(await req.Content.ReadAsStringAsync(ct)).EscapeMarkup()}[/]");

        var resp = await base.SendAsync(req, ct);
        AnsiConsole.MarkupLine($"[blue]⬅️  {(int)resp.StatusCode} {resp.ReasonPhrase.EscapeMarkup()}[/]");
        return resp;
    }
}

public record ConfigModel(OpenAIConfig OpenAI);
public record OpenAIConfig(string ApiKey);