using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class SupportRuleApplicator : IRuleApplicator
{
    public string RuleName => "support";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefaultRecursive(
            x => x.PathEquals("support.md") ||
                 x.PathEquals("docs/support.md") ||
                 x.PathEquals(".github/support.md") ||
                 x.PathEquals("support") ||
                 x.PathEquals("docs/support") ||
                 x.PathEquals(".github/support")
                 
                 );
        var (diagnosis, note) = GetDiagnosis(node);

        return new Rule
        {
            Diagnosis = diagnosis,
            Note = note,
            Name = RuleName,
            Category = Category,
            Explanation = new Explanation
            {
                Details = null,
                Text = @"
You can create a SUPPORT file to let people know about ways to get help with your project.
To direct people to specific support resources, you can add a SUPPORT file to your repository's root, docs, or .github folder. 
When someone creates an issue in your repository, they will see a link to your project's SUPPORT file.
",
                AboutUrl = "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project",
                AboutHeader = "about support files",
                GuidanceUrl =
                    "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project",
                GuidanceHeader = "how to add a support file"
            }
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
                : (Diagnosis.Warning, "missing support file");
        }
    }
}