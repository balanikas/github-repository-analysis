using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class SupportRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why are the benefits of having a github support file?";
    [RuleGuidance] private const string Example = "What is a simple example of a github support file?";
    [RuleGuidance] private const string LengthAndFormat = "How long should a github repository support file be?";
    [RuleGuidance] private const string HowTo = "How to create a great github support file, what extension should it have and and where to keep it?";
    [RuleGuidance] private const string WhatIs = "What is the purpose of github repository support file?";

    private readonly IGpt3Client _gpt3Client;

    public SupportRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "support";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.SingleFileOrDefaultRecursive(
                x => x.PathEquals("support.md") ||
                     x.PathEquals("docs/support.md") ||
                     x.PathEquals(".github/support.md") ||
                     x.PathEquals("support") ||
                     x.PathEquals("docs/support") ||
                     x.PathEquals(".github/support")
            );

            return node is not null
                ? node.Item.Size switch
                {
                    < 100 => new RuleDiagnostics(Diagnosis.Warning, "content is too short"),
                    _ => new RuleDiagnostics(Diagnosis.Info, "found")
                }
                : new RuleDiagnostics(Diagnosis.Warning, "missing support file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance, LengthAndFormat, Example, HowTo),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about support files",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project"),
            GuidanceLink = new Link("how to add a support file",
                "https://docs.github.com/en/communities/setting-up-your-project-for-healthy-contributions/adding-support-resources-to-your-project")
        });
    }
}