using Moq;
using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.Rules;
using RepositoryAnalysis.Internal.Rules.Security;

namespace Repository.Tests;

public class SecurityPolicyRuleTests
{
    [Theory]
    [MemberData(nameof(Data))]
    internal async Task Test(
        IGetRepo_Repository repo,
        Rule expected)
    {
        var tree = new GitTree(new TreeResponse("", "", Array.Empty<TreeItem>(), false));
        var result = await new SecurityPolicyRuleApplicator().ApplyAsync(new AnalysisContext(tree, repo));
        result.Should().BeEquivalentTo(expected,
            o => o.Excluding(x => x.Diagnostics.Details).Excluding(x => x.Details).Excluding(x => x.Explanation).ComparingByMembers<Rule>());
    }

    public static IEnumerable<object?[]> Data() =>
        new List<object?[]>
        {
            new object?[]
            {
                WithPolicyDisabled().Object,
                new Rule(new RuleDiagnostics(Diagnosis.Warning, "missing security policy file"))
                {
                    Category = RuleCategory.Security,
                    Name = "security policy",
                    Explanation = default!
                }
            },
            new object?[]
            {
                WithPolicyEnabled().Object,
                new Rule(new RuleDiagnostics(Diagnosis.Info, "found security policy", null, new Link("security policy", "http://test.com/url")))
                {
                    Category = RuleCategory.Security,
                    Name = "security policy",
                    Explanation = default!
                }
            }
        };

    private static Mock<IGetRepo_Repository> WithPolicyDisabled()
    {
        var repo = RepoMock();
        repo.Setup(x => x.IsSecurityPolicyEnabled)
            .Returns(false);

        return repo;
    }

    private static Mock<IGetRepo_Repository> WithPolicyEnabled()
    {
        var repo = RepoMock();
        repo.Setup(x => x.IsSecurityPolicyEnabled)
            .Returns(true);
        repo.Setup(x => x.SecurityPolicyUrl).Returns(new Uri("http://test.com/url"));

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