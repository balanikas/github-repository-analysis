using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules.Documentation;
using RepositoryAnalysis.Internal.TextGeneration;

namespace Repository.Tests;

public class ReadmeRuleTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        Diagnosis diagnosis,
        GitTree tree)
    {
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));
        var result = await new ReadmeRuleApplicator(Mock.Of<IGpt3Client>()).ApplyAsync(new AnalysisContext(tree, repo.Object));
        result.Diagnosis.Should().Be(diagnosis);
    }

    public static IEnumerable<object[]> Data() =>
        new List<object[]>
        {
            new object[]
            {
                Diagnosis.Info,
                CreateTree(
                    ("readme", TreeType.Blob, 1000))
            },
            new object[]
            {
                Diagnosis.Info,
                CreateTree(
                    ("readme.md", TreeType.Blob, 1000))
            },
            new object[]
            {
                Diagnosis.Warning,
                CreateTree(
                    ("readme.md", TreeType.Blob, 100))
            },
            new object[]
            {
                Diagnosis.Error,
                CreateTree(
                    ("folder", TreeType.Tree, 0),
                    ("folder/readme.md", TreeType.Blob, 1000))
            },
            new object[]
            {
                Diagnosis.Info,
                CreateTree(
                    ("readme.md", TreeType.Blob, 1000),
                    ("folder/readme.md", TreeType.Blob, 1000))
            }
        };

    private static GitTree CreateTree(params (string, TreeType, int)[] items) =>
        new(new TreeResponse("", "", items.Select(x => new TreeItem(x.Item1, "", x.Item2, x.Item3, "", "")).ToArray(), false));
}