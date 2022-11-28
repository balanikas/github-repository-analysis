namespace RepositoryAnalysis.Internal;

public class AnalysisContext
{
    public AnalysisContext(
        GitHubGraphQlClient graphQlClient,
        GitHubRestClient restClient)
    {
        GraphQlClient = graphQlClient;
        RestClient = restClient;
    }

    public GitHubGraphQlClient GraphQlClient { get; }
    public GitHubRestClient RestClient { get; }
    public IReadOnlyList<GitHubGraphQlClient.Entry> RootEntries { get; private set; }
    public GitHubGraphQlClient.Repo Repo { get; private set; }

    public async Task Build(
        string owner,
        string name)
    {
        Repo = await GraphQlClient.GetRepoData(owner, name);
        var repoTree = await GraphQlClient.GetRepoTree(owner, name, Repo.DefaultBranchRef.Name, string.Empty, Repo.DiskUsage);
        RootEntries = repoTree.Object.Entries;
    }
}