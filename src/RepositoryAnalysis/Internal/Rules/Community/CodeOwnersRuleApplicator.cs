using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CodeOwnersRuleApplicator : IRuleApplicator
{
    public string RuleName => "code owners";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.SingleFileOrDefaultRecursive(x => x.HasFileName("codeowners"));
            return node is not null
                ? context.Repo.Codeowners != null && context.Repo.Codeowners.Errors.Any()
                    ? new(Diagnosis.Warning, $"{context.Repo.Codeowners.Errors.Count} errors in {node.Item.Path}", null,
                        node.GetLink(context))
                    : new RuleDiagnostics(Diagnosis.Info, "", null, node.GetLink(context))
                : new(Diagnosis.Warning, "missing code owners file");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
You can use a CODEOWNERS file to define individuals or teams that are responsible for code in a repository.",
            AboutLink = new("about code owners",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners")
        });
    }
}