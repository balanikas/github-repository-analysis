using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules.Community;

namespace Repository.Tests;

public class CodeOfConductTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        GetRepo_Repository_CodeOfConduct_CodeOfConduct? coc,
        Rule expected)
    {
        var tree = new GitTree(new("", "", Array.Empty<TreeItem>(), false));
        var repo = new Mock<IGetRepo_Repository>();
        repo.Setup(x => x.Url)
            .Returns(new Uri("http://dummy.com"));
        repo.Setup(x => x.DefaultBranchRef)
            .Returns(new GetRepo_Repository_DefaultBranchRef_Ref("", null, null));
        repo.Setup(x => x.CodeOfConduct)
            .Returns(coc);
        var result = await new CodeOfConductRuleApplicator().ApplyAsync(new(tree, repo.Object));
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expected, o => o.Excluding(x => x.Explanation).Excluding(x => x.Diagnostics.Details));
    }

    public static IEnumerable<object?[]> Data() =>
        new List<object?[]>
        {
            new object?[]
            {
                new GetRepo_Repository_CodeOfConduct_CodeOfConduct(new("http://a.b"), "coc"),
                new Rule(new(Diagnosis.Info, "found", null, new("coc", "http://a.b/")))
                {
                    Category = RuleCategory.Community,
                    Name = "code of conduct",
                    Explanation = default!
                }
            },
            new object?[]
            {
                null,
                new Rule(new(Diagnosis.Warning, "missing code of conduct file"))
                {
                    Category = RuleCategory.Community,
                    Name = "code of conduct",
                    Explanation = default!
                }
            }
        };
}