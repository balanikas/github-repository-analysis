using System.Collections.Concurrent;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class AnalysisCache
{
    private static readonly ConcurrentDictionary<string, RepoAnalysis> Cache = new();
    private readonly GitHubGraphQlClient _client;

    public AnalysisCache(
        GitHubGraphQlClient client)
    {
        _client = client;
    }

    public async Task<RepoAnalysis?> Get(
        string owner,
        string name)
    {
        var repo = await _client.GetUpdatedAt(owner, name);
        if (!Cache.ContainsKey(owner + name)) return null;
        var value = Cache[owner + name];
        return value.UpdatedAt < repo.UpdatedAt ? null : value;
    }

    public RepoAnalysis Add(
        string owner,
        string name,
        RepoAnalysis analysis)
    {
        return Cache.AddOrUpdate(owner + name, _ => analysis, (
            _,
            _) => analysis);
    }
}