using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class SupportRuleApplicator : IRuleApplicator
{
    public string RuleName => "support";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.SingleFileOrDefaultRecursive(
                x => x.PathEquals("support.md") ||
                     x.PathEquals("docs/support.md") ||
                     x.PathEquals(".github/support.md") ||
                     x.PathEquals("support") ||
                     x.PathEquals("docs/support") ||
                     x.PathEquals(".github/support")
            );

            return node is not null
                ? node.Item.Size switch
                {
                    < 100 => new(Diagnosis.Warning, "content is too short"),
                    _ => new(Diagnosis.Info, "found")
                }
                : new RuleDiagnostics(Diagnosis.Warning, "missing support file");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
You can create a SUPPORT file to let people know about ways to get help with your project.
To direct people to specific support resources, you can add a SUPPORT file to your repository's root, docs, or .github folder. 
When someone creates an issue in your repository, they will see a link to your project's SUPPORT file.
",
            AboutLink = new("about support files",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project"),
            GuidanceLink = new("how to add a support file",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project")
        });
    }
}