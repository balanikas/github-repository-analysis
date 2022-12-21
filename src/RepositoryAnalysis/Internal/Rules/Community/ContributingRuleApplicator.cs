using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class ContributingRuleApplicator : IRuleApplicator
{
    public string RuleName => "contributing";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefault(
            x => x.PathEquals(
                "contributing.md",
                "docs/contributing.md",
                ".github/contributing.md",
                "contributing.rst",
                "docs/contributing.rst",
                ".github/contributing.rst"
            ));

        var (diagnosis, note) = GetDiagnosis(node);
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
To help your project contributors do good work, you can add a file with contribution guidelines to your project repository's root, docs, or .github folder. 
When someone opens a pull request or creates an issue, they will see a link to that file. The link to the contributing guidelines also appears on your repository's contribute page.",
                AboutUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors",
                AboutHeader = "about contributing guidelines",
                GuidanceUrl = node is null ? Path.Combine(context.Repo.Url.ToString(), "community") : null,
                GuidanceHeader = "Community Standards"
            },
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e)
        {
            return e is not null
                ? e.Item.Size switch
                {
                    < 100 => (Diagnosis.Warning, "content is too short"),
                    _ => (Diagnosis.Info, "found")
                }
                : (Diagnosis.Warning, "missing contributing file");
        }
    }
}