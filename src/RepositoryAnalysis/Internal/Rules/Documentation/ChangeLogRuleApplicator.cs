using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class ChangeLogRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important to have a changelog in a github repository?";
    [RuleGuidance] private const string Format = "What format should a changelog have in a github repository?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of a changelog in a github repository?";

    private readonly IGpt3Client _gpt3Client;

    public ChangeLogRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "changelog";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.FirstFileOrDefault(
                x => x.PathEndsWith("changelog.md", "change_log.md", "releasenotes.md", "release_notes.txt", "changelog.txt", "change_log.txt",
                    "releasenotes.txt",
                    "release_notes.txt"));
            var release = context.Repo.Releases.Edges?.SingleOrDefault()?.Node;

            if (node is not null)
                return new RuleDiagnostics(Diagnosis.Info, "found", null, node.GetLink(context));
            return release is not null
                ? new RuleDiagnostics(Diagnosis.Info, "found", null, new Link(release.Name!, release.Url.ToString()))
                : new RuleDiagnostics(Diagnosis.Warning, "missing");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, Format),
            Text = await _gpt3Client.GetCompletion(WhatIs)
        });
    }
}