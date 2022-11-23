using System.Net;
using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Community;
using RepositoryAnalysis.Documentation;
using RepositoryAnalysis.LanguageSpecific;
using RepositoryAnalysis.Model;
using RepositoryAnalysis.Overview;
using RepositoryAnalysis.Quality;
using RepositoryAnalysis.Security;

namespace RepositoryAnalysis;

public class AnalysisService
{
    private static readonly Dictionary<string, IReadOnlyList<string>> _urls = new();
    private readonly GitHubGraphQlClient _gitHubGraphQlClient;
    private readonly GitHubRestClient _gitHubRestClient;
    private readonly ILogger<AnalysisService> _logger;

    public AnalysisService(
        ILogger<AnalysisService> logger,
        GitHubGraphQlClient gitHubGraphQlClient,
        GitHubRestClient gitHubRestClient)
    {
        _logger = logger;
        _gitHubGraphQlClient = gitHubGraphQlClient;
        _gitHubRestClient = gitHubRestClient;
    }

    public async Task<RepoAnalysis> GetAnalysis(
        string url)
    {
        if (!await UrlExists(url)) return RepoAnalysis.NotFound;

        var (owner, name) = ExtractUrlParts(url);

        var context = new AnalysisContext(owner, name);
        try
        {
            await context.Build(_gitHubGraphQlClient);

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during analysis of {Url}", url);
            return RepoAnalysis.Error;
        }


        var overViewAnalyzer = new OverViewAnalyzer(context);
        var documentationAnalyzer = new DocumentationAnalyzer(context);
        var qualityAnalyzer = new QualityAnalyzer(context, _gitHubRestClient);
        var communityAnalyzer = new CommunityAnalyzer(context);
        var securityAnalyzer = new SecurityAnalyzer(context);
        var langSpecificAnalyzer = new LanguageSpecificAnalyzer(context);

        var overViewTask = overViewAnalyzer.Analyze();
        var documentationTask = documentationAnalyzer.Analyze();
        var qualityTask = qualityAnalyzer.Analyze();
        var communityTask = communityAnalyzer.Analyze();
        var securityTask = securityAnalyzer.Analyze();
        var langSpecificTask = langSpecificAnalyzer.Analyze();

        try
        {
            await Task.WhenAll(documentationTask, overViewTask, qualityTask, communityTask, securityTask, langSpecificTask);
            _logger.LogInformation("Successfully analyzed {Url}", url);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during analysis of {Url}", url);
            return RepoAnalysis.Error;
        }

        return new RepoAnalysis
        {
            OverView = overViewTask.Result,
            Documentation = documentationTask.Result,
            Quality = qualityTask.Result,
            Community = communityTask.Result,
            Security = securityTask.Result,
            LanguageSpecific = langSpecificTask.Result
        };
    }

    private async Task<bool> UrlExists(
        string url)
    {
        var client = new HttpClient();
        var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new Exception("something went wrong")
        };
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
            _logger.LogError($"invalid {url}", url);
            throw new ArgumentException($"invalid url format for {url}");
        }

        var owner = paths.Skip(1).First();
        var repo = paths.Last();

        return (owner, repo);
    }

    public async Task DebugAnalysis()
    {
        var urls = await _gitHubGraphQlClient.ListReposByTopic("dotnet");
        foreach (var url in urls)
        {
            var a = await GetAnalysis(url);
            Console.WriteLine($"checking {url} ----------------------------------");
            Console.WriteLine("COMMUNITY");
            foreach (var rule in a.Community) Console.WriteLine(rule with { Explanation = null, Note = null });
            Console.WriteLine();
        }
    }

    public string GetRandomRepoUrl(
        string topic)
    {
        var index = new Random().Next(0, _urls[topic].Count - 1);
        return _urls[topic][index];
    }

    public async Task LoadReposByTopic(
        string topic)
    {
        if (!_urls.ContainsKey(topic)) _urls[topic] = await _gitHubGraphQlClient.ListReposByTopic(topic);
    }
}