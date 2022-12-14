using System.Collections.Concurrent;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class AnalysisCache
{
    private static readonly ConcurrentDictionary<string, RepoAnalysis> Cache = new();

    public RepoAnalysis? Get(
        GitHubGraphQlClient.Repo repository,
        string owner,
        string name)
    {
        if (!Cache.ContainsKey(owner + name)) return null;
        var value = Cache[owner + name];
        return (value.UpdatedAt < repository.UpdatedAt || value.PushedAt < repository.PushedAt) ? null : value;
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