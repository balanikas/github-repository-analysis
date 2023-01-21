using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class PullRequestTemplatesRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why are the benefits of having github pull request templates?";
    [RuleGuidance] private const string TypesOf = "What types of pull request templates should a repository have?";
    [RuleGuidance] private const string HowTo = "How to create great github pull request templates and where to keep them?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of github pull request templates?";

    private readonly IGpt3Client _gpt3Client;

    public PullRequestTemplatesRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "pull request templates";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (context.Repo.PullRequestTemplates != null && context.Repo.PullRequestTemplates.Any())
            {
                var names = context.Repo.PullRequestTemplates.Select(x => x.Filename);
                var templates = "Templates found: <br/>" + string.Join("<br/>", names);
                return new RuleDiagnostics(Diagnosis.Info,
                    $"found {context.Repo.PullRequestTemplates.Count} pull request templates", templates);
            }

            return new RuleDiagnostics(Diagnosis.Warning, "missing pull request templates");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, TypesOf, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about pull requests",
                "https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests"),
            GuidanceLink = new Link("how to add a pull request template",
                "https://docs.github.com/en/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository")
        });
    }
}