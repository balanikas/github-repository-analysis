using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class ContributingRuleApplicator : IRuleApplicator
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

        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            return node is not null
                ? node.Item.Size switch
                {
                    < 100 => new(Diagnosis.Warning, "content is too short", null, node.GetLink(context)),
                    _ => new(Diagnosis.Info, "found", null, node.GetLink(context))
                }
                : new RuleDiagnostics(Diagnosis.Warning, "missing contributing file");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
To help your project contributors do good work, you can add a file with contribution guidelines to your project repository's root, docs, or .github folder. 
When someone opens a pull request or creates an issue, they will see a link to that file. The link to the contributing guidelines also appears on your repository's contribute page.",
            AboutLink = new("about contributing guidelines",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors"),
            GuidanceLink = node is null ? context.GetCommunityLink() : null
        });
    }
}