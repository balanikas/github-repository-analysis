using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Internal;
using RepositoryAnalysis.Model;

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
        if (!await _repositoryVerifier.RepositoryExists(url)) return RepoAnalysis.NotFound;

        var (owner, name) = ExtractUrlParts(url);

        var cachedAnalysis = await _cache.Get(owner, name);
        if (cachedAnalysis is not null)
        {
            _logger.LogInformation("Fetched analysis of {Url} from cache", url);
            return cachedAnalysis;
        }

        try
        {
            await _context.Build(owner, name);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching repository data for {Url}", url);
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
            _logger.LogInformation("Starting analysis of {Url}", url);
            await Task.WhenAll(documentationTask, qualityTask, communityTask, securityTask, langSpecificTask);

            _logger.LogRules(documentationTask.Result, url);
            _logger.LogRules(qualityTask.Result, url);
            _logger.LogRules(communityTask.Result, url);
            _logger.LogRules(securityTask.Result, url);
            _logger.LogRules(langSpecificTask.Result, url);

            _logger.LogInformation("Successfully analyzed {Url}", url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during analysis of {Url}", url);
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
            UpdatedAt = _context.Repo.UpdatedAt
        };

        return _cache.Add(owner, name, analysis);
    }

    private (string, string) ExtractUrlParts(
        string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            _logger.LogError("invalid {Url}", url);
            throw new ArgumentException($"invalid url {url}");
        }

        var paths = uri.AbsolutePath.Split("/");
        if (paths.Length != 3)
        {
            _logger.LogError("invalid {Url}", url);
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