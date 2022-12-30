using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class RulesetRuleApplicator : IRuleApplicator
{
    public string RuleName => "ruleset";
    public RuleCategory Category => RuleCategory.LanguageSpecific;
    public Language Language => Language.CSharp;

    public async Task<Rule> ApplyAsync(
        AnalysisContext context) => await Task.FromResult(Apply(context));

    private Rule Apply(
        AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var nodes = context.GitTree.FilesRecursive(x => x.HasExtension(".ruleset"));

            if (nodes.Any())
            {
                var details = $@"
Found rulesets:
<br/>
{string.Join("<br/>", nodes.Select(x => x.GetUrl(context)))}
";
                return new(Diagnosis.Warning, $"found {nodes.Count} .ruleset files", details);
            }

            return new(Diagnosis.Info, "did not find ruleset files");
        }

        return Rule.Create(this, diagnostics, new()
        {
            Text = @"
Ruleset configuration files for static code analysis are being deprecated in favor of more modern tools, 
like EditorConfig and dotnet analyzers.",
            AboutLink = new("about dotnet analyzers", "https://learn.microsoft.com/en-us/visualstudio/code-quality/analyzers-faq?view=vs-2022")
        });
    }
}