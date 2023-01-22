using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class DescriptionRuleApplicator : IRuleApplicator
{
    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is the purpose of a github repository description?";

    private readonly IGpt3Client _gpt3Client;

    public DescriptionRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;


    public string RuleName => "description";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = context.Repo.Description is not null
            ? new RuleDiagnostics(Diagnosis.Info, "found")
            : new RuleDiagnostics(Diagnosis.Warning, "missing");

        return Rule.Create(this, diagnostics, new Explanation
        {
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("this guide on how to create a repository", "https://docs.github.com/en/get-started/quickstart/create-a-repo"),
            GuidanceLink = diagnostics.Diagnosis == Diagnosis.Warning ? context.GetCommunityLink() : null
        });
    }
}