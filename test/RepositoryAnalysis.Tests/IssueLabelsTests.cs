using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules.Community;

namespace Repository.Tests;

public class IssueLabelsTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        IGetRepo_Repository repo,
        Diagnosis diagnosis)
    {
        var tree = new GitTree(new TreeResponse("", "", Array.Empty<TreeItem>(), false));

        var result = await new IssueLabelsRuleApplicator().ApplyAsync(new AnalysisContext(tree, repo));
        result.Should().NotBeNull();
        result.Diagnosis.Should().Be(diagnosis);
    }

    public static IEnumerable<object?[]> Data() =>
        new List<object?[]>
        {
            new object?[]
            {
                WithIssueLabels().Object,
                Diagnosis.Info
            },
            new object?[]
            {
                WithIssuesDisabled().Object,
                Diagnosis.NotApplicable
            },
            new object?[]
            {
                WithNoIssues().Object,
                Diagnosis.NotApplicable
            },
            new object?[]
            {
                WithNoLabels().Object,
                Diagnosis.Warning
            }
        };

    private static Mock<IGetRepo_Repository> WithIssueLabels()
    {
        var repo = RepoMock();
        repo.Setup(x => x.HasIssuesEnabled)
            .Returns(true);
        repo.Setup(x => x.Issues).Returns(new GetRepo_Repository_Issues_IssueConnection(
            new List<IGetRepo_Repository_Issues_Edges?>
            {
                new GetRepo_Repository_Issues_Edges_IssueEdge(
                    new GetRepo_Repository_Issues_Edges_Node_Issue(
                        new GetRepo_Repository_Issues_Edges_Node_Labels_LabelConnection(3), 123, new DateTimeOffset()))
            }));

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithNoLabels()
    {
        var repo = RepoMock();
        repo.Setup(x => x.HasIssuesEnabled)
            .Returns(true);
        repo.Setup(x => x.Issues).Returns(new GetRepo_Repository_Issues_IssueConnection(
            new List<IGetRepo_Repository_Issues_Edges?>
            {
                new GetRepo_Repository_Issues_Edges_IssueEdge(
                    new GetRepo_Repository_Issues_Edges_Node_Issue(
                        new GetRepo_Repository_Issues_Edges_Node_Labels_LabelConnection(0), 123, new DateTimeOffset()))
            }));

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithIssuesDisabled()
    {
        var repo = RepoMock();
        repo.Setup(x => x.HasIssuesEnabled)
            .Returns(false);

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithNoIssues()
    {
        var repo = RepoMock();
        repo.Setup(x => x.HasIssuesEnabled)
            .Returns(true);
        repo.Setup(x => x.Issues).Returns(new GetRepo_Repository_Issues_IssueConnection(
            new List<IGetRepo_Repository_Issues_Edges?>()
        ));

        return repo;
    }

    private static Mock<IGetRepo_Repository> RepoMock()
    {
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));

        return repo;
    }
}