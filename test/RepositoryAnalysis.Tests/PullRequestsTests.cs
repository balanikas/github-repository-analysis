using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules.Community;

namespace Repository.Tests;

public class PullRequestsTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        Diagnosis diagnosis,
        GetRepo_Repository_PullRequests_PullRequestConnection prs)
    {
        var tree = new GitTree(new("", "", Array.Empty<TreeItem>(), false));
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));
        repo.Setup(x => x.PullRequests).Returns(prs);

        var result = await new PullRequestsRuleApplicator().ApplyAsync(new(tree, repo.Object));
        result.Diagnosis.Should().Be(diagnosis);
    }

    public static IEnumerable<object[]> Data() =>
        new List<object[]>
        {
            _Test(Diagnosis.Info,
                new DateTimeOffset(DateTime.UtcNow.AddDays(-1)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(-1))
            ),
            _Test(Diagnosis.Info
            ),
            _Test(Diagnosis.Warning,
                new DateTimeOffset(DateTime.UtcNow.AddDays(-100)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(-1))
            ),
            _Test(Diagnosis.Warning,
                new DateTimeOffset(DateTime.UtcNow.AddDays(-40)),
                new DateTimeOffset(DateTime.UtcNow.AddDays(-40))
            )
        };

    private static object[] _Test(
        Diagnosis diagnosis,
        params DateTimeOffset[] dates) =>
        new object[]
        {
            diagnosis,
            new GetRepo_Repository_PullRequests_PullRequestConnection(
                dates.Select(x => new GetRepo_Repository_PullRequests_Nodes_PullRequest(x)).ToList()
            )
        };
}

