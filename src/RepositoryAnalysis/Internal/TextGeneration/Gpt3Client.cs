using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace RepositoryAnalysis.Internal.TextGeneration;

public class Gpt3Client : IGpt3Client
{
    private readonly ILogger<Gpt3Client> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly OpenAIService _openAiService;

    public Gpt3Client(
        ILogger<Gpt3Client> logger,
        IMemoryCache memoryCache,
        HttpClient httpClient)
    {
        _logger = logger;
        _memoryCache = memoryCache;

        var options = new OpenAiOptions
        {
            ApiKey = Environment.GetEnvironmentVariable("OpenAI__Token")
        };

        _openAiService = new OpenAIService(options, httpClient);
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