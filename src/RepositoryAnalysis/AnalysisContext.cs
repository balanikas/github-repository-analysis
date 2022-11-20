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


    public IReadOnlyList<GitHubApi.Entry> RootEntries { get; private set; }

    public GitHubApi.Repo Repo { get; private set; }


    public async Task Build(
        GitHubApi api)
    {
        Repo = await api.GetRepoData(_owner, _name);
        RootEntries = await api.GetRepoTree(_owner, _name, Repo.DefaultBranchRef.Name, string.Empty, Repo.DiskUsage);
    }
}