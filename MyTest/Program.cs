using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using OpenAI.Responses;
using System.Text.Json;

// Load configuration
var config = LoadConfig();

await CallResponseApi_OpenAI_WebSearch(config);

await CallResponsesApi_OpenAI(config);

await CallResponsesApi_Local();


static async Task CallResponseApi_OpenAI_WebSearch(ConfigModel config)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    System.Console.WriteLine("\n\n\nUsing the Semantic Kernel to use the Responses API to call OpenAI to do a web search about Unicorns...");
    Console.ResetColor();

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
    var responseItems = agent.InvokeStreamingAsync("Do a web search for information about unicorns, summarize in three sentences.", options: invokeOptions);
    await foreach (var responseItem in responseItems)
    {
        Console.Write(responseItem.Message.Content);
    }
}

static async Task CallResponsesApi_Local()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    System.Console.WriteLine("\n\n\nUsing Semantic Kernel to create another story about unicorns, using the experimental support for the Responses API in the Huggingface Transformers library, running locally in Docker...");
    Console.ResetColor();

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
    var responseItems = agent.InvokeStreamingAsync("Tell me a three sentence bedtime story about a unicorn.");

    await foreach (var responseItem in responseItems)
    {
        Console.Write(responseItem.Message.Content);
    }

    System.Console.WriteLine("\n\n\n");
}

static async Task CallResponsesApi_OpenAI(ConfigModel config)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    System.Console.WriteLine("\n\n\nUsing the Responses API in OpenAI to create a story about unicorns...");
    Console.ResetColor();

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
    var responseItems = agent.InvokeStreamingAsync("Tell me a three sentence bedtime story about a unicorn.");

    await foreach (var responseItem in responseItems)
    {
        Console.Write(responseItem.Message.Content);
    }
}

static ConfigModel LoadConfig()
{
    try
    {
        var configPath = "config.json";
        if (!File.Exists(configPath))
        {
            Console.WriteLine($"Config file {configPath} not found. Please create it with your API keys.");
            Environment.Exit(1);
        }

        var configJson = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<ConfigModel>(configJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config?.OpenAI?.ApiKey == null)
        {
            Console.WriteLine("OpenAI API key not found in config file.");
            Environment.Exit(1);
        }

        return config;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading config: {ex.Message}");
        Environment.Exit(1);
        return null; // Never reached
    }
}

public sealed class LoggingHandler : DelegatingHandler
{
    public LoggingHandler(HttpMessageHandler inner) : base(inner) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
    {
        Console.WriteLine($"➡️ {req.Method} {req.RequestUri}");
        foreach (var h in req.Headers)
        {
            if (h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"{h.Key}: [MASKED]");
            }
            else
            {
                Console.WriteLine($"{h.Key}: {string.Join(",", h.Value)}");
            }
        }
        if (req.Content != null) Console.WriteLine(await req.Content.ReadAsStringAsync(ct));

        var resp = await base.SendAsync(req, ct);
        Console.WriteLine($"⬅️ {(int)resp.StatusCode} {resp.ReasonPhrase}");
        return resp;
    }
}

public record ConfigModel(OpenAIConfig OpenAI);
public record OpenAIConfig(string ApiKey);