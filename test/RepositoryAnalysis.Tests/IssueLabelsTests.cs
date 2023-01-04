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
        Rule expected)
    {
        var tree = new GitTree(new("", "", Array.Empty<TreeItem>(), false));

        var result = await new IssueLabelsRuleApplicator().ApplyAsync(new(tree, repo));
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected,
            o => o.Excluding(x => x.Diagnostics.Details).Excluding(x => x.Diagnostics.Details).Excluding(x => x.Explanation).ComparingByMembers<Rule>());
    }

    public static IEnumerable<object?[]> Data() =>
        new List<object?[]>
        {
            new object?[]
            {
                WithIssueLabels().Object,
                new Rule(new(Diagnosis.Info, "issues are enabled and all open issues are labeled"))
                {
                    Category = RuleCategory.Community,
                    Name = "issue labels",
                    Explanation = default!
                }
            },
            new object?[]
            {
                WithIssuesDisabled().Object,
                new Rule(new(Diagnosis.NotApplicable, "feature is disabled"))
                {
                    Category = RuleCategory.Community,
                    Name = "issue labels",
                    Explanation = default!
                }
            },
            new object?[]
            {
                WithNoIssues().Object,
                new Rule(new(Diagnosis.NotApplicable, "no open issues"))
                {
                    Category = RuleCategory.Community,
                    Name = "issue labels",
                    Explanation = default!
                }
            },
            new object?[]
            {
                WithNoLabels().Object,
                new Rule(new(Diagnosis.Warning, "found 100% of 1 issues unlabeled"))
                {
                    Category = RuleCategory.Community,
                    Name = "issue labels",
                    Explanation = default!
                }
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
                        new GetRepo_Repository_Issues_Edges_Node_Labels_LabelConnection(3), 123, new()))
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
                        new GetRepo_Repository_Issues_Edges_Node_Labels_LabelConnection(0), 123, new()))
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