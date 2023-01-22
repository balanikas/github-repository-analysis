using RepositoryAnalysis.Internal.GraphQL;
using RepositoryAnalysis.Internal.TextGeneration;
using RepositoryAnalysis.Model;

namespace RepositoryAnalysis.Internal.Rules.Community;

internal class IssueLabelsRuleApplicator : IRuleApplicator
{
    [RuleGuidance] private const string IdealLabelCount = "How many labels should a github issue ideally have?";
    [RuleGuidance] private const string BadIdea = "Why is it a bad idea to not label a github issue?";
    [RuleGuidance] private const string TooManyLabels = "Can a github issue have too many labels?";

    [RuleGuidance(200, Tone.Motivational, Complexity.Simple)]
    private const string WhatIs = "What are issue labels in a github repository?";

    [RuleGuidance] private const string Process = "What is the process of adding a label to a github issue?";

    private readonly IGpt3Client _gpt3Client;

    public IssueLabelsRuleApplicator(IGpt3Client gpt3Client) => _gpt3Client = gpt3Client;

    public string RuleName => "issue labels";
    public RuleCategory Category => RuleCategory.Community;
    public Language Language => Language.None;

    public async Task<Rule> ApplyAsync(AnalysisContext context)
    {
        var diagnostics = GetDiagnosis();

        RuleDiagnostics GetDiagnosis()
        {
            if (!context.Repo.HasIssuesEnabled) return new RuleDiagnostics(Diagnosis.NotApplicable, "feature is disabled");
            if (context.Repo.Issues.Edges is null || !context.Repo.Issues.Edges.Any()) return new RuleDiagnostics(Diagnosis.NotApplicable, "no open issues");

            var nodes = context.Repo.Issues.Edges
                .Select(x => x!.Node!)
                .ToArray();
            var nodesWithoutLabels = nodes
                .Where(x => x.Labels is not null && x.Labels.TotalCount == 0)
                .ToArray();

            if (nodesWithoutLabels.Length <= 0)
                return new RuleDiagnostics(
                    Diagnosis.Info,
                    "issues are enabled and all open issues are labeled",
                    null,
                    new Link("issues", Path.Combine(context.Repo.Url.ToString(), "issues")));

            var percent = (int)Math.Round((double)(100 * nodesWithoutLabels.Length) / nodes.Length);
            var details = $"""
Sample of unlabeled issues: <br/>{string.Join("<br/>", nodesWithoutLabels.Take(5).Select(x => GetUrl(x, context)))}
""";
            return new RuleDiagnostics(Diagnosis.Warning, $"found {percent}% of {nodes.Length} issues unlabeled", details,
                new Link("unlabeled issues", Path.Combine(context.Repo.Url.ToString(), "issues?q=is%3Aopen+is%3Aissue+no%3Alabel")));
        }

        return Rule.Create(this, diagnostics, new Explanation
        {
            GeneralGuidance = await _gpt3Client.GetCompletions(BadIdea, IdealLabelCount, TooManyLabels, Process),
            Text = await _gpt3Client.GetCompletion(WhatIs),
            AboutLink = new Link("about issue labels", "https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels")
        });
    }

    private string GetUrl(
        IGetRepo_Repository_Issues_Edges_Node node,
        AnalysisContext context) =>
        Shared.CreateEmbeddedLink(Path.Combine(context.Repo.Url.ToString(), "issues", node.Number.ToString()), "issue " + node.Number);
}