using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class LicenseRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important to carefully choose a license for a github repository?";
    [RuleGuidance] private const string HowTo = "How to create a great github repository license file, in what format and where to put it?";
    [RuleGuidance] private const string LicenseType = "How to choose a license for a github repository?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is the purpose of a license file in a github repository?";

    private readonly IGpt3Client _gpt3Client;

    public LicenseRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "license";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = context.Repo.LicenseInfo is not null
            ? new RuleDiagnostics(Diagnosis.Info, "found", null,
                new Link(context.Repo.LicenseInfo.Name, context.Repo.LicenseInfo.Url?.ToString() ?? string.Empty))
            : new RuleDiagnostics(Diagnosis.Error, "missing");

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, LicenseType, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about open source licensing",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Error ? context.GetCommunityLink() : null
        });
    }
}