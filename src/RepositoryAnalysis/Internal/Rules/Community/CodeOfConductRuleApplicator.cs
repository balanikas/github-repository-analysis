using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CodeOfConductRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important for a github repository to have a code of conduct?";
    [RuleGuidance] private const string HowTo = "How to write a great code of conduct file?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of a code of conduct in open source?";

    private readonly IGpt3Client _gpt3Client;

    public CodeOfConductRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "code of conduct";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = context.Repo.CodeOfConduct is not null
            ? new RuleDiagnostics(Diagnosis.Info, "found", null,
                new Link(context.Repo.CodeOfConduct.Name, context.Repo.CodeOfConduct.Url!.ToString()))
            : new RuleDiagnostics(Diagnosis.Warning, "missing code of conduct file");

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = new Dictionary<string, string>
            {
                { Importance, await _gpt3Client.GetCompletion(Importance) },
                { HowTo, await _gpt3Client.GetCompletion(HowTo) }
            },
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about code of conduct",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-a-code-of-conduct-to-your-project"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}