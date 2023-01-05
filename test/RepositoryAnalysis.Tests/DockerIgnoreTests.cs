using Moq;
using RepositoryAnalysis.Internal.GraphQL;

namespace Repository.Tests;

public class DockerIgnoreTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        GitTree tree,
        Diagnosis diagnosis)
    {
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));
        var result = await new DockerIgnoreRuleApplicator().ApplyAsync(new AnalysisContext(tree, repo.Object));
        result.Diagnosis.Should().Be(diagnosis);
    }

    public static IEnumerable<object[]> Data() =>
        new List<object[]>
        {
            _Test(Diagnosis.Info,
                ("Dockerfile", TreeType.Blob),
                (".dockerignore", TreeType.Blob)),
            _Test(Diagnosis.Info,
                ("Dockerfile", TreeType.Blob),
                ("f", TreeType.Tree),
                ("f/.dockerignore", TreeType.Blob)),
            _Test(Diagnosis.Info,
                ("f", TreeType.Tree),
                ("f/Dockerfile", TreeType.Blob),
                (".dockerignore", TreeType.Blob)),
            _Test(Diagnosis.Warning,
                (".dockerignore", TreeType.Blob)),
            _Test(Diagnosis.Warning,
                ("Dockerfile", TreeType.Blob))
        };

    private static object[] _Test(
        Diagnosis diagnosis,
        params (string, TreeType)[] items) =>
        new object[]
        {
            new GitTree(new TreeResponse("", "", items.Select(x => new TreeItem(x.Item1, "", x.Item2, 0, "", "")).ToArray(), false)),
            diagnosis
        };
}