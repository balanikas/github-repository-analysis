using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Documentation;

internal class TopicsRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important for a github repository to have topics?";
    [RuleGuidance] private const string IdealTopicCount = "What is the ideal number of topics for a github repository?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is a github topic?";

    private readonly IGpt3Client _gpt3Client;

    public TopicsRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "topics";
    public RuleCategory Category => RuleCategory.Documentation;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = context.Repo.RepositoryTopics.TotalCount > 0
            ? new RuleDiagnostics(Diagnosis.Info, $"found {context.Repo.RepositoryTopics.TotalCount} topics")
            : new RuleDiagnostics(Diagnosis.Warning, "no topics found");

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, IdealTopicCount),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about topics",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics"),
            GuidanceLink = new Link("how to work with topics",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics")
        });
    }
}