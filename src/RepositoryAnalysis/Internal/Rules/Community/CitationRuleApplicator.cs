using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CitationRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string WhenAppropriate = "When is it appropriate and when unappropriate to use citation files in github repositories?";
    [RuleGuidance] private const string HowTo = "How to write an open source citations file?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of citations in open source?";

    private readonly IGpt3Client _gpt3Client;

    public CitationRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "citation";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var citationFileNames = new[]
            {
                "CITATION",
                "CITATIONS",
                "CITATION.bib",
                "CITATIONS.bib",
                "CITATION.md",
                "CITATIONS.md",
                "CITATION.cff"
            };
            var node = context.GitTree.FirstFileOrDefault(
                x => citationFileNames.Contains(x.Item.Path, StringComparer.OrdinalIgnoreCase));
            return node is not null
                ? new RuleDiagnostics(Diagnosis.Info, "found")
                : new RuleDiagnostics(Diagnosis.Warning, "missing citation file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(WhenAppropriate, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about citation files",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-citation-files")
        });
    }
}