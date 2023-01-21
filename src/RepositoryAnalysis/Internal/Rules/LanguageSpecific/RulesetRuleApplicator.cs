using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.LanguageSpecific;

internal class RulesetRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string WhatIs = "What is the purpose of a .ruleset file in a .net solution?";
    [RuleGuidance] private const string Alternatives = "What is the best way to ensure high code quality standards in a .net solution?";
    [RuleGuidance] private const string IsLegacy = "Is .ruleset files for static code analysis in .net solutions considered legacy?";

    private readonly IGpt3Client _gpt3Client;

    public RulesetRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "ruleset";
    public RuleCategory Category => RuleCategory.LanguageSpecific;
    public Language Language => Language.CSharp;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
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
{nodes.GetEmbeddedLinksAsString(context)}
";
                return new RuleDiagnostics(Diagnosis.Warning, $"found {nodes.Count} .ruleset files", details);
            }

            return new RuleDiagnostics(Diagnosis.Info, "did not find ruleset files");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(IsLegacy, Alternatives),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about dotnet analyzers", "https://learn.microsoft.com/en-us/visualstudio/code-quality/analyzers-faq?view=vs-2022")
        });
    }
}