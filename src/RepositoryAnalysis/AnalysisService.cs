using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Internal;
using RepositoryAnalysis.Internal.Rules;
using RepositoryAnalysis.Model;
using Serilog.Context;
using OverView = RepositoryAnalysis.Internal.OverView;

namespace RepositoryAnalysis;

public class AnalysisService
{
    private readonly IEnumerable<IAnalyzer> _analyzers;
    private readonly AnalysisCache _cache;
    private readonly AnalysisContext _context;
    private readonly GitHubGraphQlClient _gitHubGraphQlClient;
    private readonly ILogger<AnalysisService> _logger;
    private readonly OverView _overView;

    public AnalysisService(
        ILogger<AnalysisService> logger,
        GitHubGraphQlClient gitHubGraphQlClient,
        AnalysisCache cache,
        AnalysisContext context,
        OverView overView,
        IEnumerable<IAnalyzer> analyzers)
    {
        _logger = logger;
        _gitHubGraphQlClient = gitHubGraphQlClient;
        _cache = cache;
        _context = context;
        _overView = overView;
        _analyzers = analyzers;
    }

    public async Task<RepoAnalysis> GetAnalysis(
        string url)
    {
        using var _ = LogContext.PushProperty("RepositoryUrl", url);
        _logger.LogInformation("Starting analysis.");

        //todo: unecessary call, remove
        //if (!await _repositoryVerifier.RepositoryExists(url)) return RepoAnalysis.NotFound;

        var (owner, name) = ExtractUrlParts(url);

        var repository = await _context.GraphQlClient.GetUpdatedAt(owner, name);
        if (repository is null) return RepoAnalysis.NotFound;

        var cachedAnalysis = _cache.Get(repository, owner, name);
        if (cachedAnalysis is not null)
        {
            _logger.LogInformation("Fetched analysis from cache");
            return cachedAnalysis;
        }

        try
        {
            await _context.Build(owner, name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while building analysis context.");
            return RepoAnalysis.Error;
        }

        var overView = _overView.Analyze(_context);
        var tasks = _analyzers.Select(x => x.Analyze(_context)).ToArray();
        Rule[] allRules;
        try
        {
            await Task.WhenAll(tasks);
            allRules = tasks.SelectMany(x => x.Result).ToArray();
            _logger.LogRules(allRules, url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during analysis.");
            return RepoAnalysis.Error;
        }

        var analysis = new RepoAnalysis
        {
            OverView = overView,
            Documentation = allRules.Where(x => x.Category == RuleCategory.Documentation).ToArray(),
            Quality = allRules.Where(x => x.Category == RuleCategory.Quality).ToArray(),
            Community = allRules.Where(x => x.Category == RuleCategory.Community).ToArray(),
            Security = allRules.Where(x => x.Category == RuleCategory.Security).ToArray(),
            LanguageSpecific = allRules.Where(x => x.Category == RuleCategory.LanguageSpecific).ToArray(),
            UpdatedAt = _context.Repo.UpdatedAt,
            Issues = _context.GetIssues()
        };

        return _cache.Add(owner, name, analysis);
    }

    private (string, string) ExtractUrlParts(
        string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _logger.LogError("invalid url {Url}", url);
            throw new ArgumentException($"invalid url {url}");
        }

        var paths = uri.AbsolutePath.Split("/");
        if (paths.Length != 3)
        {
            _logger.LogError("invalid url {Url}", url);
            throw new ArgumentException($"invalid url format for {url}");
        }

        var owner = paths.Skip(1).First();
        var repo = paths.Last();

        return (owner, repo);
    }

    public async Task DoBulkTest(
        string topic)
    {
        var test = new BulkAnalysisTest();
        await test.Run(
            topic,
            x => _gitHubGraphQlClient.ListReposByTopic(x),
            GetAnalysis);
    }
}