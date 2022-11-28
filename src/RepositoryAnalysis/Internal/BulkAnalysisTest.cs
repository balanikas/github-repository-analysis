using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal;

public class BulkAnalysisTest
{
    private static readonly Dictionary<string, List<string>> _urls = new();

    public async Task Run(
        string topic,
        Func<string, Task<GitHubGraphQlClient.ListRepos.Topic>> func,
        Func<string, Task<RepoAnalysis>> analysisFunc)
    {
        await LoadReposByTopic(topic, func);
        string? repo;
        do
        {
            repo = GetRandomRepoUrl();
            if (repo is not null)
                await analysisFunc(repo);
        } while (repo is not null);
    }

    private string? GetRandomRepoUrl()
    {
        if (_urls.Count == 0) return null;

        var keys = _urls.Keys.ToList();
        var keyIndex = new Random().Next(0, keys.Count - 1);
        var randomTopic = keys[keyIndex];
        var repos = _urls[randomTopic];

        var repoIndex = new Random().Next(0, repos.Count - 1);
        var randomRepo = repos[repoIndex];
        repos.RemoveAt(repoIndex);
        if (repos.Count == 0) _urls.Remove(randomTopic);

        return randomRepo;
    }

    private async Task LoadReposByTopic(
        string topic,
        Func<string, Task<GitHubGraphQlClient.ListRepos.Topic>> func)
    {
        if (_urls.Count > 10) return;
        await Task.Delay(1000);
        GitHubGraphQlClient.ListRepos.Topic response;
        try
        {
            response = await func(topic);
        }
        catch (Exception)
        {
            return;
        }

        if (!_urls.ContainsKey(topic))
        {
            _urls[topic] = response.repositories.edges.Select(x => x.node.url).ToList();
            Console.WriteLine("added topic " + topic);
        }

        await Task.WhenAll(response.relatedTopics.Select(related => LoadReposByTopic(related.name, func)).ToArray());
    }
}