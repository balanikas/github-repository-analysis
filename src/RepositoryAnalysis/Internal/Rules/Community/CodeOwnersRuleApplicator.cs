using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

public class CodeOwnersRuleApplicator : IRuleApplicator
{
    public string RuleName => "code owners";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefaultRecursive(x => x.HasFileName("codeowners"));

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
You can use a CODEOWNERS file to define individuals or teams that are responsible for code in a repository.",
                AboutUrl =
                    "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners",
                AboutHeader = "about code owners"
            },
            ResourceName = node?.Item.Path, ResourceUrl = node.GetUrl(context)
        };

        (Diagnosis, string) GetDiagnosis(
            GitTree.Node? e) =>
            e is not null
                ? context.Repo.Codeowners.Errors.Any()
                    ? (Diagnosis.Warning, $"{context.Repo.Codeowners.Errors.Count} errors in {e.Item.Path}")
                    : (Diagnosis.Info, "")
                : (Diagnosis.Warning, "missing code owners file");
    }
}