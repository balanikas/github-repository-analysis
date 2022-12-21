using RepositoryAnalysis.Internal.GraphQL;

namespace RepositoryAnalysis.Internal;

public class AnalysisContext
{
    public AnalysisContext(
        GitTree gitTree,
        IGetRepo_Repository repo)
    {
        GitTree = gitTree;
        Repo = repo;
    }

    public AnalysisContext(
        GitHubGraphQlClient graphQlClient,
        GitHubRestClient restClient)
    {
        GraphQlClient = graphQlClient;
        RestClient = restClient;
        GitTree = new GitTree();
    }

    public GitHubGraphQlClient GraphQlClient { get; }
    public GitHubRestClient RestClient { get; }
    public IGetRepo_Repository Repo { get; private set; }
    public GitTree GitTree { get; private set; }

    public async Task Build(
        string owner,
        string name)
    {
        Repo = await GraphQlClient.GetRepository(owner, name);
        GitTree = await RestClient.GetGitTree(owner, name, Repo.DefaultBranchRef!.Target!.CommitResourcePath.ToString());
    }

    public IReadOnlyList<string> GetIssues()
    {
        return GitTree.Truncated
            ? new[] { $"Repository contains {GitTree.Count} files and is too large. Some rules might not function properly" }
            : Array.Empty<string>();
    }
}