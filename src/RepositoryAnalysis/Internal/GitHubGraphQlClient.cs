using Microsoft.Extensions.Logging;
using RepositoryAnalysis.Internal.GraphQL;

namespace RepositoryAnalysis.Internal;

internal class GitHubGraphQlClient
{
    private readonly GithubClient _githubClient;
    private readonly ILogger<GitHubGraphQlClient> _logger;

    public GitHubGraphQlClient(
        ILogger<GitHubGraphQlClient> logger,
        GithubClient githubClient)
    {
        _logger = logger;
        _githubClient = githubClient;
    }

    public async Task<IGetAge_Repository?> GetUpdatedAt(
        string owner,
        string name)
    {
        var result = await _githubClient.GetAge.ExecuteAsync(name, owner);
        if (result.Errors.Any()) throw new Exception($"Could not get repository {owner}/{name}", result.Errors[0].Exception);

        return result.Data?.Repository;
    }

    public async Task<IGetRepo_Repository> GetRepository(
        string owner,
        string name)
    {
        var result = await _githubClient.GetRepo.ExecuteAsync(name, owner);

        if (result.Errors.Any())
        {
            _logger.LogError(result.Errors[0].Exception, $"Could not get repository {owner}/{name}");
            throw new Exception($"Could not get repository {owner}/{name}", result.Errors[0].Exception);
        }

        if (result.Data?.Repository is null)
        {
            _logger.LogError($"Could not get repository {owner}/{name}");
            throw new Exception($"Could not get repository {owner}/{name}");
        }

        return result.Data.Repository;
    }
}

