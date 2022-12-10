using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Internal;
using RepositoryAnalysis.Model;
using Serilog.Context;

namespace RepositoryAnalysis;

public class AnalysisService
{
    private readonly AnalysisCache _cache;
    private readonly CommunityAnalyzer _communityAnalyzer;
    private readonly AnalysisContext _context;
    private readonly DocumentationAnalyzer _documentationAnalyzer;
    private readonly GitHubGraphQlClient _gitHubGraphQlClient;
    private readonly LanguageSpecificAnalyzer _languageSpecificAnalyzer;
    private readonly ILogger<AnalysisService> _logger;
    private readonly OverViewAnalyzer _overViewAnalyzer;
    private readonly QualityAnalyzer _qualityAnalyzer;
    private readonly RepositoryVerifier _repositoryVerifier;
    private readonly SecurityAnalyzer _securityAnalyzer;

    public AnalysisService(
        ILogger<AnalysisService> logger,
        GitHubGraphQlClient gitHubGraphQlClient,
        AnalysisCache cache,
        RepositoryVerifier repositoryVerifier,
        AnalysisContext context,
        OverViewAnalyzer overViewAnalyzer,
        DocumentationAnalyzer documentationAnalyzer,
        LanguageSpecificAnalyzer languageSpecificAnalyzer,
        QualityAnalyzer qualityAnalyzer,
        CommunityAnalyzer communityAnalyzer,
        SecurityAnalyzer securityAnalyzer)
    {
        _logger = logger;
        _gitHubGraphQlClient = gitHubGraphQlClient;
        _cache = cache;
        _repositoryVerifier = repositoryVerifier;
        _context = context;
        _overViewAnalyzer = overViewAnalyzer;
        _documentationAnalyzer = documentationAnalyzer;
        _languageSpecificAnalyzer = languageSpecificAnalyzer;
        _qualityAnalyzer = qualityAnalyzer;
        _communityAnalyzer = communityAnalyzer;
        _securityAnalyzer = securityAnalyzer;
    }

    public async Task<RepoAnalysis> GetAnalysis(
        string url)
    {
        using var _ = LogContext.PushProperty("RepositoryUrl", url);
        _logger.LogInformation("Starting analysis.");
    
        if (!await _repositoryVerifier.RepositoryExists(url)) return RepoAnalysis.NotFound;

        var (owner, name) = ExtractUrlParts(url);

        var cachedAnalysis = await _cache.Get(owner, name);
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

        var overView = _overViewAnalyzer.Analyze(_context);
        var documentationTask = _documentationAnalyzer.Analyze(_context);
        var qualityTask = _qualityAnalyzer.Analyze(_context);
        var communityTask = _communityAnalyzer.Analyze(_context);
        var securityTask = _securityAnalyzer.Analyze(_context);
        var langSpecificTask = _languageSpecificAnalyzer.Analyze(_context);

        try
        {
            await Task.WhenAll(documentationTask, qualityTask, communityTask, securityTask, langSpecificTask);

            var allRules = documentationTask.Result
                .Concat(qualityTask.Result)
                .Concat(communityTask.Result)
                .Concat(securityTask.Result)
                .Concat(langSpecificTask.Result);
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
            Documentation = documentationTask.Result,
            Quality = qualityTask.Result,
            Community = communityTask.Result,
            Security = securityTask.Result,
            LanguageSpecific = langSpecificTask.Result,
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