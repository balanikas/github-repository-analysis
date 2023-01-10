using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules.Community;

namespace Repository.Tests;

public class DiscussionsRuleTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        IGetRepo_Repository repo,
        Diagnosis diagnosis)
    {
        var tree = new GitTree(new TreeResponse("", "", Array.Empty<TreeItem>(), false));

        var result = await new DiscussionsRuleApplicator().ApplyAsync(new AnalysisContext(tree, repo));
        result.Should().NotBeNull();
        result.Diagnostics.Diagnosis.Should().Be(diagnosis);
    }

    public static IEnumerable<object?[]> Data() =>
        new List<object?[]>
        {
            new object?[]
            {
                WithDiscussionsDisabled().Object,
                Diagnosis.NotApplicable
            },
            new object?[]
            {
                WithNoDiscussions().Object,
                Diagnosis.Info
            },
            new object?[]
            {
                WithAllDiscussionsAnswered().Object,
                Diagnosis.Info
            },
            new object?[]
            {
                WithMostDiscussionsUnanswered().Object,
                Diagnosis.Warning
            },
            new object?[]
            {
                WithMostDiscussionsAnswered().Object,
                Diagnosis.Info
            }
        };

    private static Mock<IGetRepo_Repository> WithDiscussionsDisabled()
    {
        var repo = RepoMock();
        repo.Setup(x => x.HasDiscussionsEnabled)
            .Returns(false);

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithNoDiscussions()
    {
        var repo = RepoMock();
        repo.Setup(x => x.Discussions).Returns(
            new GetRepo_Repository_Discussions_DiscussionConnection(
                new List<IGetRepo_Repository_Discussions_Edges?>(), 0));

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithAllDiscussionsAnswered()
    {
        var repo = RepoMock();
        repo.Setup(x => x.Discussions).Returns(
            new GetRepo_Repository_Discussions_DiscussionConnection(
                new List<IGetRepo_Repository_Discussions_Edges?>
                {
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(
                        new GetRepo_Repository_Discussions_Edges_Node_Discussion(1,
                            new GetRepo_Repository_Discussions_Edges_Node_Answer_DiscussionComment(true))),
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(
                        new GetRepo_Repository_Discussions_Edges_Node_Discussion(2,
                            new GetRepo_Repository_Discussions_Edges_Node_Answer_DiscussionComment(true)))
                }, 2));

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithMostDiscussionsAnswered()
    {
        var repo = RepoMock();
        repo.Setup(x => x.Discussions).Returns(
            new GetRepo_Repository_Discussions_DiscussionConnection(
                new List<IGetRepo_Repository_Discussions_Edges?>
                {
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(
                        new GetRepo_Repository_Discussions_Edges_Node_Discussion(1,
                            new GetRepo_Repository_Discussions_Edges_Node_Answer_DiscussionComment(true))),
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(new GetRepo_Repository_Discussions_Edges_Node_Discussion(2, null)),
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(
                        new GetRepo_Repository_Discussions_Edges_Node_Discussion(3,
                            new GetRepo_Repository_Discussions_Edges_Node_Answer_DiscussionComment(true)))
                }, 3));

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithMostDiscussionsUnanswered()
    {
        var repo = RepoMock();
        repo.Setup(x => x.Discussions).Returns(
            new GetRepo_Repository_Discussions_DiscussionConnection(
                new List<IGetRepo_Repository_Discussions_Edges?>
                {
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(new GetRepo_Repository_Discussions_Edges_Node_Discussion(1, null)),
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(new GetRepo_Repository_Discussions_Edges_Node_Discussion(2, null)),
                    new GetRepo_Repository_Discussions_Edges_DiscussionEdge(
                        new GetRepo_Repository_Discussions_Edges_Node_Discussion(3,
                            new GetRepo_Repository_Discussions_Edges_Node_Answer_DiscussionComment(true)))
                }, 3));

        return repo;
    }

    private static Mock<IGetRepo_Repository> RepoMock()
    {
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));
        repo.Setup(x => x.HasDiscussionsEnabled)
            .Returns(true);
        return repo;
    }
}