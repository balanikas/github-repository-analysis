using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Internal.Rules;

namespace RepositoryAnalysis.Internal.TextGeneration;

public class TextGenerationService : BackgroundService
{
    private readonly IGpt3Client _gpt3Client;
    private readonly ILogger<TextGenerationService> _logger;

    public TextGenerationService(
        ILogger<TextGenerationService> logger,
        IGpt3Client gpt3Client)
    {
        _logger = logger;
        _gpt3Client = gpt3Client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(TextGenerationService)} is starting.");

        stoppingToken.Register(() => _logger.LogDebug("Text generation background task is stopping."));

        if (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Text generation task doing background work.");

            await FetchAllAssemblyCompletions();
            await Task.Delay(1000, stoppingToken);
        }

        _logger.LogInformation("Text generation background task is stopping.");
    }

    private async Task FetchAllAssemblyCompletions()
    {
        var applicator = typeof(IRuleApplicator);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => applicator.IsAssignableFrom(p));

        var prompts = types
            .SelectMany(x => x.GetFields(BindingFlags.Static | BindingFlags.NonPublic))
            .Where(x => x.GetCustomAttributes().OfType<RuleGuidanceAttribute>().Count() == 1)
            .Select(x => x.GetRawConstantValue()).Cast<string>().ToArray();

        await Parallel.ForEachAsync(prompts, async (
            p,
            _) =>
        {
            await _gpt3Client.GetCompletion(p);
        });

        _logger.LogInformation("Finished completion generation");
    }
}