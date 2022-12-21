using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;

namespace RepositoryAnalysis.Internal;

internal class GitHubRestClient
{
    private static readonly ConcurrentDictionary<string, Ignore.Ignore> CachedTemplates = new();

    private readonly GitHubClient _client;
    private readonly ILogger<GitHubGraphQlClient> _logger;

    public GitHubRestClient(
        IOptions<GitHubOptions> githubOptions,
        ILogger<GitHubGraphQlClient> logger)
    {
        _logger = logger;
        _client = new GitHubClient(new ProductHeaderValue("github-repository-analysis"))
        {
            Credentials = new Credentials(Environment.GetEnvironmentVariable("GitHub__Token"))
        };
    }

    public async Task<(string, Ignore.Ignore)> GetGitIgnoreRules(
        string language)
    {
        if (CachedTemplates.TryGetValue(language, out var value)) return (GetTemplateName(language), value);

        GitIgnoreTemplate template;
        try
        {
            template = await _client.GitIgnore.GetGitIgnoreTemplate(GetTemplateName(language));
        }
        catch (ApiException e)
        {
            _logger.LogError(e, "Error fetching git ignore template for language {Language}", language);
            return (GetTemplateName(language), new Ignore.Ignore());
        }

        var rules = template.Source.Split("\n");
        var ignore = new Ignore.Ignore();
        ignore.Add(rules);
        return (GetTemplateName(language), CachedTemplates.GetOrAdd(language, ignore));
    }

    private static string GetTemplateName(
        string language)
    {
        return language.ToLower() switch
        {
            "c#" => "VisualStudio",
            "python" => "Python",
            _ => language
        };
    }

    public async Task<GitTree> GetGitTree(
        string owner,
        string name,
        string commitResourcePath)
    {
        var sha = Path.GetFileName(commitResourcePath);
        var response = await _client.Git.Tree.GetRecursive(owner, name, sha);
        var contents = new GitTree(response);
        return contents;
    }
}