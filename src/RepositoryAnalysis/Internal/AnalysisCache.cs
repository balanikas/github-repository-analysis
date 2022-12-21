using System.Collections.Concurrent;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

internal class AnalysisCache
{
    private static readonly ConcurrentDictionary<string, RepoAnalysis> Cache = new();

    public RepoAnalysis? Get(
        IGetAge_Repository repository,
        string owner,
        string name)
    {
        if (!Cache.ContainsKey(owner + name)) return null;
        var value = Cache[owner + name];
        return value.UpdatedAt < repository.UpdatedAt.DateTime || value.PushedAt < repository.PushedAt.Value.DateTime ? null : value;
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