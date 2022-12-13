using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

public class RulesetRuleApplicator : IRuleApplicator
{
    public string RuleName => "ruleset";
    public RuleCategory Category => RuleCategory.LanguageSpecific;
    public Language Language => Language.CSharp;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var nodes = context.GitTree.FilesRecursive(x => x.HasExtension(".ruleset"));
        var details = nodes.Any()
            ? $@"
Found these rulesets:
<br/>
{string.Join("<br/>", nodes.Select(x => x.GetUrl(context)))}
"
            : "";
        var (diagnosis, note) = GetDiagnosis(nodes);
        return new Rule
        {
            Name = RuleName,
            Category = Category,
            Note = note,
            Diagnosis = diagnosis,
            Explanation = new Explanation
            {
                Details = details,
                Text = @"
Ruleset configuration files for static code analysis are being deprecated in favor of more modern tools, 
like EditorConfig and dotnet analyzers.",
                AboutUrl = "https://learn.microsoft.com/en-us/visualstudio/code-quality/analyzers-faq?view=vs-2022",
                AboutHeader = "about dotnet analyzers"
            }
        };

        (Diagnosis, string) GetDiagnosis(
            IReadOnlyList<GitTree.Node> e) =>
            e.Any()
                ? (Diagnosis.Warning, $"found {e.Count} .ruleset files")
                : (Diagnosis.Info, "did not find ruleset files");
    }
}