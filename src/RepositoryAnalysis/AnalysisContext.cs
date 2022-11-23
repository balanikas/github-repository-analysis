namespace RepositoryAnalysis;

public class AnalysisContext
{
    private readonly string _name;
    private readonly string _owner;

    public AnalysisContext(
        string owner,
        string name)
    {
        _owner = owner;
        _name = name;
    }


    public IReadOnlyList<GitHubGraphQlClient.Entry> RootEntries { get; private set; }

    public GitHubGraphQlClient.Repo Repo { get; private set; }
    public GitHubGraphQlClient.Repo RepoTree { get; private set; }


    public async Task Build(
        GitHubGraphQlClient graphQlClient)
    {
        Repo = await graphQlClient.GetRepoData(_owner, _name);
        var repoTree = await graphQlClient.GetRepoTree(_owner, _name, Repo.DefaultBranchRef.Name, string.Empty, Repo.DiskUsage);
        RootEntries = repoTree.Object.Entries;
        RepoTree = repoTree;
    }
}