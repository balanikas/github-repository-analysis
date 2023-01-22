using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class ContributingRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important for a github repository to have a contributing file?";
    [RuleGuidance] private const string HowTo = "How to write a great contributing file?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is the purpose of a contributing file in open source?";

    private readonly IGpt3Client _gpt3Client;

    public ContributingRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "contributing";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var node = context.GitTree.SingleFileOrDefault(
            x => x.PathEquals(
                "contributing.md",
                "docs/contributing.md",
                ".github/contributing.md",
                "contributing.rst",
                "docs/contributing.rst",
                ".github/contributing.rst"
            ));

        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            return node is not null
                ? node.Item.Size switch
                {
                    < 100 => new RuleDiagnostics(Diagnosis.Warning, "content is too short", null, node.GetLink(context)),
                    _ => new RuleDiagnostics(Diagnosis.Info, "found", null, node.GetLink(context))
                }
                : new RuleDiagnostics(Diagnosis.Warning, "missing contributing file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about contributing guidelines",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/setting-guidelines-for-repository-contributors"),
            GuidanceLink = node is null ? context.GetCommunityLink() : null
        });
    }
}