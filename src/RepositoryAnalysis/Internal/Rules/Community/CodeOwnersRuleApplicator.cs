using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class CodeOwnersRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string Importance = "Why is it important to have a codeowners file in a github repository?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What is the purpose of a codeowners file in a github repository?";

    private readonly IGpt3Client _gpt3Client;

    public CodeOwnersRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "code owners";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            var node = context.GitTree.SingleFileOrDefaultRecursive(x => x.HasFileName("codeowners"));
            return node is not null
                ? context.Repo.Codeowners != null && context.Repo.Codeowners.Errors.Any()
                    ? new RuleDiagnostics(Diagnosis.Warning, $"{context.Repo.Codeowners.Errors.Count} errors in {node.Item.Path}", null,
                        node.GetLink(context))
                    : new RuleDiagnostics(Diagnosis.Info, "", null, node.GetLink(context))
                : new RuleDiagnostics(Diagnosis.Warning, "missing code owners file");
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(Importance),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about code owners",
                "https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners")
        });
    }
}