using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal.TextGeneration;

public class Gpt3Client : IGpt3Client
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<Gpt3Client> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly OpenAIService _openAiService;
    private readonly Dictionary<string, RuleGuidanceAttribute> _promptMetaDataMap = new();

    public Gpt3Client(
        ILogger<Gpt3Client> logger,
        IMemoryCache memoryCache,
        HttpClient httpClient,
        IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _hostEnvironment = hostEnvironment;

        var options = new OpenAiOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OpenAI__Token")
        };

        _openAiService = new OpenAIService(options, httpClient);
    }

    public void AddPromptMetaData(
        string prompt,
        RuleGuidanceAttribute promptMetaData)
    {
        _promptMetaDataMap[prompt] = promptMetaData;
    }

    public async Task<IDictionary<string, string>> GetCompletions(params string[] prompts)
    {
        var completions = new Dictionary<string, string>();
        foreach (var prompt in prompts) completions.Add(prompt, await GetCompletion(prompt));

        return completions;
    }

    public async Task<string> GetCompletion(string prompt)
    {
        if (_memoryCache.TryGetValue(prompt, out string? cacheValue)) return cacheValue!;

        var value = await GetCompletionInternal(prompt);
        if (string.IsNullOrEmpty(value)) return value;

        value = value.Trim('\n').Replace("\n", "<br/>");
        _memoryCache.Set(prompt, value);
        return value;
    }

    private async Task<string> GetCompletionInternal(string prompt)
    {
        if (_hostEnvironment.IsDevelopment()) return string.Join("", Enumerable.Repeat("loren ipsum ", new Random().Next(2, 50)));

        var meta = _promptMetaDataMap[prompt];

        prompt = new PromptBuilder(prompt, meta)
            .WithContext()
            .WithComplexity()
            .WithTone()
            .WithLength()
            .ToString();

        Console.WriteLine(prompt);

        var request = new CompletionCreateRequest
        {
            Prompt = prompt,
            MaxTokens = 500,
            Temperature = 1f,
            TopP = 1f,
            FrequencyPenalty = 2f,
            PresencePenalty = 2f
        };

        var completionResult = await _openAiService.Completions.CreateCompletion(request, Models.TextDavinciV3);
        if (completionResult.Successful) return completionResult.Choices.FirstOrDefault()?.Text ?? "";

        if (completionResult.Error == null)
        {
            _logger.LogError("Unknown error calling openai completion endpoint.");
            throw new Exception("Unknown error calling openai completion endpoint.");
        }

        _logger.LogError($"Error calling openai completion endpoint. {completionResult.Error.Code}: {completionResult.Error.Message}");

        return "";
    }
}