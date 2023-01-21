using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class IssueTemplatesRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why are the benefits of having github issue templates?";
    [RuleGuidance] private const string TypesOf = "What types of issue templates should a repository have?";
    [RuleGuidance] private const string HowTo = "How to create great github issue templates and where to keep them?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of github issue templates?";

    private readonly IGpt3Client _gpt3Client;

    public IssueTemplatesRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "issue templates";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (context.Repo.HasIssuesEnabled)
                if (context.Repo.IssueTemplates != null && context.Repo.IssueTemplates.Any())
                {
                    var names = context.Repo.IssueTemplates.Select(x => x.Name);
                    var templates = $"Templates found: <br/>{string.Join("<br/>", names)}";
                    return new RuleDiagnostics(Diagnosis.Info, $"issues are enabled and found {context.Repo.IssueTemplates.Count} issue templates", templates);
                }
                else
                {
                    return new RuleDiagnostics(Diagnosis.Warning, "issues are enabled but missing issue templates");
                }

            return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, TypesOf, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about issues", "https://docs.github.com/en/issues/tracking-your-work-with-issues/about-issues"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}