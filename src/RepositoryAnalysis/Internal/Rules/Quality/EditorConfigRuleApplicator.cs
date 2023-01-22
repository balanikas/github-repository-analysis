using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Quality;

internal class EditorConfigRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string WhereToFind = "Where can i find a good editorconfig file for my repository?";
    [RuleGuidance] private const string MultipleFiles = "When should i have multiple editorconfig files in my repository?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is editorconfig and why should i add it to a repository?";

    [RuleGuidance] private const string CheckValidity = "How can i check that an editorconfig file is valid?";

    private readonly IGpt3Client _gpt3Client;

    public EditorConfigRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "editorconfig";
    public RuleCategory Category => RuleCategory.Quality;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var rootFile = context.GitTree.FirstFileOrDefault(x => x.PathEquals(".editorconfig"));
            var nonRootFiles = context.GitTree.FilesRecursive(x => x.PathEndsWith(".editorconfig"));
            if (rootFile is not null)
                return nonRootFiles.Count > 1
                    ? new RuleDiagnostics(Diagnosis.Info, $"found at root and {nonRootFiles.Count} files at other locations", null, rootFile.GetLink(context))
                    : new RuleDiagnostics(Diagnosis.Info, "found at root", null, rootFile.GetLink(context));

            return nonRootFiles.Count > 0
                ? new RuleDiagnostics(Diagnosis.Info, $"missing at root but found {nonRootFiles.Count} files at other locations")
                : new RuleDiagnostics(Diagnosis.Error, "missing editorconfig file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(WhereToFind, MultipleFiles, CheckValidity),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about editor config", "https://editorconfig.org/")
        });
    }
}